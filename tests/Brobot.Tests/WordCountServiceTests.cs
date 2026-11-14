using Brobot.Models;
using Brobot.Services;
using Brobot.Tests.TestHelpers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brobot.Tests;

[TestFixture]
public class WordCountServiceTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PageDelay = WordCountService.PageDelay;

    private Mock<ILogger<WordCountService>> _loggerMock;
    private Mock<IDiscordClient> _clientMock;
    private Mock<IStopWordService> _stopWordServiceMock;
    private ObservableTimeProvider _timeProvider;
    private WordCountService _service;
    private ChannelModel _channel;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<WordCountService>>();
        _clientMock = new Mock<IDiscordClient>();
        _stopWordServiceMock = new Mock<IStopWordService>();
        // Only drives the delay between page requests. Message timestamps and the time window in these tests
        // use the real clock, which is fine because WordCountService never reads the time itself.
        _timeProvider = new ObservableTimeProvider(DateTimeOffset.UtcNow);
        _service = new WordCountService(_loggerMock.Object, _clientMock.Object, _stopWordServiceMock.Object, _timeProvider);
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

        _stopWordServiceMock.Setup(s => s.IsStopWord("stopword")).ReturnsAsync(true);
        _stopWordServiceMock.Setup(s => s.IsStopWord(It.IsNotIn(new[] { "stopword" }))).ReturnsAsync(false);

        var result = (await RunWithFakeTime(_service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow))).ToList();

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
        
        var result = (await RunWithFakeTime(_service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1)))).ToList();

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
            
        _stopWordServiceMock.Setup(s => s.IsStopWord("hello")).ReturnsAsync(false);
        _stopWordServiceMock.Setup(s => s.IsStopWord("world")).ReturnsAsync(false);
        _stopWordServiceMock.Setup(s => s.IsStopWord("again")).ReturnsAsync(false);
        _stopWordServiceMock.Setup(s => s.IsStopWord("latest")).ReturnsAsync(false);
        _stopWordServiceMock.Setup(s => s.IsStopWord("message")).ReturnsAsync(false);
        
        var result = (await RunWithFakeTime(_service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1)))).ToList();

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
    public async Task GetWordCount_WhenExceptionOccurs_LogsErrorAndReturnsEmpty()
    {
        // Arrange
        Mock<ILogger<WordCountService>> mockLogger = new();
        Mock<IDiscordClient> mockDiscordClient = new();
        Mock<IStopWordService> mockStopWordService = new();
        // Throws before the first page request, so no delay is ever reached
        WordCountService wordCountService = new(mockLogger.Object, mockDiscordClient.Object, mockStopWordService.Object, TimeProvider.System);
        GuildModel guild = new()
        {
            Id = 1UL,
            Name = "Test Guild"
        };
        ChannelModel channel = new()
        {
            Id = 2UL,
            Name = "Test Channel",
            Guild = guild,
            GuildId = 1UL
        };
        mockDiscordClient.Setup(dc => dc.GetChannelAsync(channel.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ThrowsAsync(new Exception());
        
        // Act
        var result = await wordCountService.GetWordCount(channel, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
        
        // Assert
        using (Assert.EnterMultipleScope())
        {
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    ((Func<It.IsAnyType, Exception, string>)It.IsAny<object>())!), Times.Once);
            Assert.That(result, Is.Empty);
        }
    }

    [Test]
    public async Task GetWordCount_ReturnsCount_ExcludesNumbers()
    {
        var messages = new List<IMessage>
        {
            CreateMockMessage(1UL, "Hello 1234 world!"),
            CreateMockMessage(2UL, "Hello again."),
            CreateMockMessage(3UL, "69534")
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

        _stopWordServiceMock.Setup(s => s.IsStopWord(It.IsAny<string>())).ReturnsAsync(false);

        var result = (await RunWithFakeTime(_service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow))).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Sum(r => r.Count), Is.EqualTo(4));
            Assert.That(result.Any(r => r is { Word: "hello", Count: 2 }), Is.True);
            Assert.That(result.Any(r => r is { Word: "world", Count: 1 }), Is.True);
            Assert.That(result.Any(r => r.Word == "1234"), Is.False);
            Assert.That(result.Any(r => r.Word == "69534"), Is.False);
        }
    }

    [Test]
    public async Task GetWordCount_WhenCancellationTokenIsCanceled_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        
        Assert.That(async () => await _service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Never);
    }

    [Test]
    public async Task GetWordCount_WhenMessageHasCurlyApostrophes_NormalizesWord()
    {
        var messages = new List<IMessage>
        {
            CreateMockMessage(1UL, "Hello world!"),
            CreateMockMessage(2UL, "It’s a beautiful day."),
            CreateMockMessage(3UL, "Don’t stop believing.")
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

        _stopWordServiceMock.Setup(s => s.IsStopWord("don't")).ReturnsAsync(true);
        _stopWordServiceMock.Setup(s => s.IsStopWord(It.IsNotIn(new[] { "don't" }))).ReturnsAsync(false);

        var result = (await RunWithFakeTime(_service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow))).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Count, Is.EqualTo(8));
            Assert.That(result.Sum(r => r.Count), Is.EqualTo(8));
            Assert.That(result.Any(r => r is { Word: "it's", Count: 1 }), Is.True);
            Assert.That(result.Any(r => r.Word == "don't"), Is.False);
            Assert.That(result.Any(r => r.Word == "it’s"), Is.False);
            Assert.That(result.Any(r => r.Word == "don’t"), Is.False);
        }
    }

    [Test]
    public async Task GetWordCount_WaitsPageDelayBeforeEachRequestAfterTheFirst_IncludingAfterPageNewerThanWindow()
    {
        // Page 1: entirely newer than the window (the case that used to skip the delay)
        // Page 2: inside the window
        // Page 3: empty, ends the loop
        var page1 = new List<IMessage> { CreateMockMessage(3UL, "Latest message", DateTimeOffset.UtcNow.AddDays(-1).AddHours(6)) };
        var page2 = new List<IMessage> { CreateMockMessage(2UL, "Hello again.", DateTimeOffset.UtcNow.AddDays(-2).AddHours(2)) };
        var textChannelMock = new Mock<ISocketMessageChannel>();
        textChannelMock
            .Setup(c => c.GetMessagesAsync(It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(CreateMockAsyncEnumerable(page1));
        textChannelMock
            .Setup(c => c.GetMessagesAsync(3UL, Direction.Before, It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(CreateMockAsyncEnumerable(page2));
        textChannelMock
            .Setup(c => c.GetMessagesAsync(2UL, Direction.Before, It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(CreateMockAsyncEnumerable([]));
        _clientMock.Setup(c => c.GetChannelAsync(_channel.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(textChannelMock.Object);
        _stopWordServiceMock.Setup(s => s.IsStopWord(It.IsAny<string>())).ReturnsAsync(false);

        var task = _service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));

        // First request goes out immediately, then the service waits before the second
        await _timeProvider.WaitForTimerCreated();
        textChannelMock.Verify(c => c.GetMessagesAsync(It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()), Times.Once);
        VerifyPageRequested(textChannelMock, 3UL, Times.Never());

        // Not yet: 1ms short of the delay
        _timeProvider.Advance(PageDelay - TimeSpan.FromMilliseconds(1));
        VerifyPageRequested(textChannelMock, 3UL, Times.Never());

        // Delay elapses -> second request, then the service waits again before the third
        _timeProvider.Advance(TimeSpan.FromMilliseconds(1));
        await _timeProvider.WaitForTimerCreated();
        VerifyPageRequested(textChannelMock, 3UL, Times.Once());
        VerifyPageRequested(textChannelMock, 2UL, Times.Never());

        _timeProvider.Advance(PageDelay);
        var result = (await task.WaitAsync(TestTimeout)).ToList();

        using (Assert.EnterMultipleScope())
        {
            VerifyPageRequested(textChannelMock, 2UL, Times.Once());
            // One delay before each of requests 2 and 3, none before the first or after the last
            Assert.That(_timeProvider.TimersCreated, Is.EqualTo(2));
            Assert.That(result.Select(r => r.Word), Is.EquivalentTo(new[] { "hello", "again" }));
        }
    }

    [Test]
    public async Task GetWordCount_WhenCancelledDuringPageDelay_ThrowsOperationCanceledException()
    {
        var page1 = new List<IMessage> { CreateMockMessage(2UL, "Hello again.", DateTimeOffset.UtcNow.AddDays(-2).AddHours(2)) };
        var textChannelMock = new Mock<ISocketMessageChannel>();
        textChannelMock
            .Setup(c => c.GetMessagesAsync(It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(CreateMockAsyncEnumerable(page1));
        _clientMock.Setup(c => c.GetChannelAsync(_channel.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(textChannelMock.Object);
        _stopWordServiceMock.Setup(s => s.IsStopWord(It.IsAny<string>())).ReturnsAsync(false);
        using var cts = new CancellationTokenSource();

        var task = _service.GetWordCount(_channel, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), cts.Token);
        await _timeProvider.WaitForTimerCreated();
        await cts.CancelAsync();

        Assert.That(async () => await task.WaitAsync(TestTimeout), Throws.InstanceOf<OperationCanceledException>());
        using (Assert.EnterMultipleScope())
        {
            textChannelMock.Verify(
                c => c.GetMessagesAsync(It.IsAny<ulong>(), It.IsAny<Direction>(), It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()),
                Times.Never);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Never);
        }
    }

    /// <summary>
    /// Awaits a GetWordCount call while advancing the fake clock past each page delay, so tests that don't care
    /// about the delay finish immediately. If an Advance lands before the next delay has started, the loop just
    /// advances again, so there's no race to lose.
    /// </summary>
    private async Task<T> RunWithFakeTime<T>(Task<T> task)
    {
        var deadline = DateTime.UtcNow + TestTimeout;
        while (!task.IsCompleted)
        {
            if (DateTime.UtcNow > deadline)
            {
                Assert.Fail("Timed out waiting for GetWordCount to finish");
            }
            _timeProvider.Advance(PageDelay);
            await Task.Yield();
        }
        return await task;
    }

    private static void VerifyPageRequested(Mock<ISocketMessageChannel> channelMock, ulong beforeMessageId, Times times) =>
        channelMock.Verify(
            c => c.GetMessagesAsync(beforeMessageId, Direction.Before, It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()),
            times);

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