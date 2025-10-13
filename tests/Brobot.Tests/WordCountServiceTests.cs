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
            CreateMockMessage(1UL, "Hello world!"),
            CreateMockMessage(2UL, "Hello again."),
            CreateMockMessage(3UL, "StopWord test")
        };
        var textChannelMock = new Mock<ISocketMessageChannel>();
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                It.IsAny<ulong>(),
                It.IsAny<Direction>(),
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(CreateMockAsyncEnumerable([]));
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(CreateMockAsyncEnumerable(messages));
        _clientMock.Setup(c => c.GetChannelAsync(_channel.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(textChannelMock.Object);

        _stopWordServiceMock.Setup(s => s.IsStopWord("StopWord")).ReturnsAsync(true);
        _stopWordServiceMock.Setup(s => s.IsStopWord(It.IsNotIn(new[] { "StopWord" }))).ReturnsAsync(false);

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
            CreateMockMessage(4UL, "Latest message", DateTimeOffset.UtcNow.AddDays(-1).AddHours(-1)),
            CreateMockMessage(3UL, "Hello again.", DateTimeOffset.UtcNow.AddDays(-2).AddHours(2)),
            CreateMockMessage(2UL, "Hello world!", DateTimeOffset.UtcNow.AddDays(-2).AddHours(1)),
        };
        var messages2 = new List<IMessage>
        {
            CreateMockMessage(1UL, "This shouldn't show", DateTimeOffset.UtcNow.AddDays(-3)),
        };
        var textChannelMock = new Mock<ISocketMessageChannel>();
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                It.IsAny<ulong>(),
                It.IsAny<Direction>(),
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(CreateMockAsyncEnumerable(messages2));
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(CreateMockAsyncEnumerable(messages1));
        _clientMock.Setup(c => c.GetChannelAsync(_channel.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(textChannelMock.Object);
        
        _stopWordServiceMock.Setup(s => s.IsStopWord(It.IsAny<string>())).ReturnsAsync(false);
        
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

    [Test]
    public async Task GetWordCount_ReturnsCounts_WithMessagesNewerThanWindow()
    {
        var messages1 = new List<IMessage>
        {
            CreateMockMessage(3UL, "Latest message", DateTimeOffset.UtcNow.AddDays(-1).AddHours(6))
        };
        var messages2 = new List<IMessage>
        {
            CreateMockMessage(2UL, "Hello again.", DateTimeOffset.UtcNow.AddDays(-2).AddHours(2)),
            CreateMockMessage(1UL, "Hello world!", DateTimeOffset.UtcNow.AddDays(-2).AddHours(1)),
        };
        var textChannelMock = new Mock<ISocketMessageChannel>();
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                3UL,
                Direction.Before,
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>())
            ).Returns(CreateMockAsyncEnumerable(messages2));
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(CreateMockAsyncEnumerable(messages1));
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                It.IsNotIn(new [] { 3UL }),
                Direction.Before,
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(CreateMockAsyncEnumerable([]));
        _clientMock.Setup(c => c.GetChannelAsync(_channel.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(textChannelMock.Object);
            
        _stopWordServiceMock.Setup(s => s.IsStopWord(It.IsAny<string>())).ReturnsAsync(false);
        
        var result = (await _service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1))).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Any(r => r is { Word: "hello", Count: 2 }), Is.True);
            Assert.That(result.Any(r => r is { Word: "world", Count: 1 }), Is.True);
            Assert.That(result.Any(r => r is { Word: "again", Count: 1 }), Is.True);
            Assert.That(result.Any(r => r is { Word: "latest", Count: 1 }), Is.False);
            Assert.That(result.Any(r => r is { Word: "message", Count: 1 }), Is.False);
        }
    }

    [Test]
    public async Task GetWordCountAsync_ReturnsCounts_IgnoresNumbers()
    {
        var messages = new List<IMessage>
        {
            CreateMockMessage(1UL, "Test Message"),
            CreateMockMessage(2UL, "4"),
            CreateMockMessage(3UL, "58239251924821")
        };
        
        var textChannelMock = new Mock<ISocketMessageChannel>();
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                It.IsAny<ulong>(),
                It.IsAny<Direction>(),
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(CreateMockAsyncEnumerable([]));
        textChannelMock
            .Setup(c => c.GetMessagesAsync(
                It.IsAny<int>(),
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(CreateMockAsyncEnumerable(messages));

        _clientMock.Setup(c => c.GetChannelAsync(_channel.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync(textChannelMock.Object);
        _stopWordServiceMock.Setup(s => s.IsStopWord(It.IsAny<string>())).ReturnsAsync(false);
        
        var result = (await _service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow)).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(r => r.Word == "4"), Is.False);
            Assert.That(result.Any(r => r.Word == "58239251924821"), Is.False);
        }
    }

    private static IAsyncEnumerable<IReadOnlyCollection<IMessage>> CreateMockAsyncEnumerable(List<IMessage> messages)
    {
        return new[] { (IReadOnlyCollection<IMessage>)messages }.ToAsyncEnumerable();
    }

    private static IMessage CreateMockMessage(ulong id, string content, DateTimeOffset? timestamp = null)
    {
        timestamp ??= DateTimeOffset.UtcNow;
        var mock = new Mock<IMessage>();
        mock.SetupGet(m => m.Id).Returns(id);
        mock.Setup(m => m.Content).Returns(content);
        mock.Setup(m => m.Timestamp).Returns(timestamp.Value);
        return mock.Object;
    }
}