using Brobot.Models;
using Brobot.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brobot.Tests;

[TestFixture]
public class WordCountServiceTests
{
    private Mock<ILogger<WordCountService>> _loggerMock;
    private Mock<IDiscordClient> _clientMock;
    private Mock<IStopWordService> _stopWordServiceMock;
    private WordCountService _service;
    private ChannelModel _channel;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<WordCountService>>();
        _clientMock = new Mock<IDiscordClient>();
        _stopWordServiceMock = new Mock<IStopWordService>();
        _service = new WordCountService(_loggerMock.Object, _clientMock.Object, _stopWordServiceMock.Object);
        _channel = new ChannelModel
        {
            Id = 123UL,
            Timezone = "America/Chicago",
            Name = "test-channel",
            Guild = new GuildModel
            {
                Id = 123UL,
                Name = "test-guild",
            }
        };
    }

    [Test]
    public async Task GetWordCount_ReturnsCounts_ExcludesStopWords()
    {
        var messages = new List<IMessage>
        {
            MockMessage(1UL, "Hello world!"),
            MockMessage(2UL, "Hello again."),
            MockMessage(3UL, "StopWord test")
        };
        var textChannelMock = new Mock<ISocketMessageChannel>();
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                It.IsAny<ulong>(),
                It.IsAny<Direction>(),
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(MockAsyncEnumerable([]));
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(MockAsyncEnumerable(messages));
        _clientMock.Setup(c => c.GetChannelAsync(_channel.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(textChannelMock.Object);

        _stopWordServiceMock.Setup(s => s.IsStopWord("Hello")).ReturnsAsync(false);
        _stopWordServiceMock.Setup(s => s.IsStopWord("world")).ReturnsAsync(false);
        _stopWordServiceMock.Setup(s => s.IsStopWord("again")).ReturnsAsync(false);
        _stopWordServiceMock.Setup(s => s.IsStopWord("StopWord")).ReturnsAsync(true);
        _stopWordServiceMock.Setup(s => s.IsStopWord("test")).ReturnsAsync(false);

        var result = (await _service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow)).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result.Sum(r => r.Count), Is.EqualTo(5));
            Assert.That(result.Any(r => r is { Word: "hello", Count: 2 }), Is.True);
            Assert.That(result.Any(r => r.Word == "stopword"), Is.False);
        }
    }

    [Test]
    public async Task GetWordCount_ReturnsEmpty_WhenNotTextChannel()
    {
        _clientMock.Setup(c => c.GetChannelAsync(_channel.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(new Mock<ISocketAudioChannel>().Object);

        var result = await _service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetWordCount_ReturnsEmpty_OnException()
    {
        _clientMock.Setup(c => c.GetChannelAsync(_channel.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(new Mock<ISocketMessageChannel>().Object);

        var result = await _service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetWordCount_ReturnsCounts_InTimeWindow()
    {
        var messages1 = new List<IMessage>
        {
            MockMessage(4UL, "Latest message", DateTimeOffset.UtcNow.AddDays(-1).AddHours(-1)),
            MockMessage(3UL, "Hello again.", DateTimeOffset.UtcNow.AddDays(-2).AddHours(2)),
            MockMessage(2UL, "Hello world!", DateTimeOffset.UtcNow.AddDays(-2).AddHours(1)),
        };
        var messages2 = new List<IMessage>
        {
            MockMessage(1UL, "This shouldn't show", DateTimeOffset.UtcNow.AddDays(-3)),
        };
        var textChannelMock = new Mock<ISocketMessageChannel>();
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                It.IsAny<ulong>(),
                It.IsAny<Direction>(),
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(MockAsyncEnumerable(messages2));
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(MockAsyncEnumerable(messages1));
        _clientMock.Setup(c => c.GetChannelAsync(_channel.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(textChannelMock.Object);
        
        _stopWordServiceMock.Setup(s => s.IsStopWord("Hello")).ReturnsAsync(false);
        _stopWordServiceMock.Setup(s => s.IsStopWord("world")).ReturnsAsync(false);
        _stopWordServiceMock.Setup(s => s.IsStopWord("again")).ReturnsAsync(false);
        _stopWordServiceMock.Setup(s => s.IsStopWord("Latest")).ReturnsAsync(false);
        _stopWordServiceMock.Setup(s => s.IsStopWord("message")).ReturnsAsync(false);
        
        var result = (await _service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1))).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result.Sum(r => r.Count), Is.EqualTo(6));
            Assert.That(result.Any(r => r is { Word: "hello", Count: 2 }), Is.True);
            Assert.That(result.Any(r => r is { Word: "world", Count: 1 }), Is.True);
            Assert.That(result.Any(r => r.Word == "this"), Is.False);
            Assert.That(result.Any(r => r.Word == "shouldn't"), Is.False);
            Assert.That(result.Any(r => r.Word == "show"), Is.False);
        }
    }

    private static IAsyncEnumerable<IReadOnlyCollection<IMessage>> MockAsyncEnumerable(List<IMessage> messages)
    {
        return new[] { (IReadOnlyCollection<IMessage>)messages }.ToAsyncEnumerable();
    }

    private static IMessage MockMessage(ulong id, string content, DateTimeOffset? timestamp = null)
    {
        timestamp ??= DateTimeOffset.UtcNow;
        var mock = new Mock<IMessage>();
        mock.SetupGet(m => m.Id).Returns(id);
        mock.Setup(m => m.Content).Returns(content);
        mock.Setup(m => m.Timestamp).Returns(timestamp.Value);
        return mock.Object;
    }
}