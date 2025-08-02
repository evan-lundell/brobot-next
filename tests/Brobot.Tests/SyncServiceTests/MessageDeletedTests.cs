using Brobot.Configuration;
using Brobot.Services;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class MessageDeletedTests : SyncServiceTestsBase
{
    private Mock<ILogger<SyncService>> _loggerMock;
    public override void Setup()
    {
        GeneralOptions generalOptions = new()
        {
            FixTwitterLinks = true,
            VersionFilePath = "./version.txt"
        };
        _loggerMock = new Mock<ILogger<SyncService>>();
        SyncService = new SyncService(
            Mock.Of<IServiceScopeFactory>(),
            Mock.Of<IDiscordClient>(),
            _loggerMock.Object,
            Options.Create(generalOptions));
    }

    public override void TearDown()
    {
    }

    protected override void SetupDatabase()
    {
        throw new NotImplementedException();
    }

    [Test]
    public async Task ChannelIsNotText_DoesNothing()
    {
        Mock<IMessage> messageMock = new();
        Mock<IMessageChannel> channelMock = new();
        Mock<IGuild> guildMock = new();
        
        channelMock.Setup(c => c.SendMessageAsync(It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<Embed>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<AllowedMentions>(),
                It.IsAny<MessageReference>(),
                It.IsAny<MessageComponent>(),
                It.IsAny<ISticker[]>(),
                It.IsAny<Embed[]>(),
                It.IsAny<MessageFlags>(),
                It.IsAny<PollProperties>()))
            .Verifiable();
        channelMock.SetupGet(c => c.ChannelType).Returns(ChannelType.Voice);
        
        await SyncService.MessageDeleted(messageMock.Object, channelMock.Object, guildMock.Object);

        channelMock.Verify(c => c.SendMessageAsync(It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<Embed>(),
            It.IsAny<RequestOptions>(),
            It.IsAny<AllowedMentions>(),
            It.IsAny<MessageReference>(),
            It.IsAny<MessageComponent>(),
            It.IsAny<ISticker[]>(),
            It.IsAny<Embed[]>(),
            It.IsAny<MessageFlags>(),
            It.IsAny<PollProperties>()), Times.Never);
    }

    [Test]
    public async Task NoMessageDeletedLogFound_RespondWithAuthorUsername()
    {
        const string username = "user";
        Mock<IMessage> messageMock = new();
        Mock<IMessageChannel> channelMock = new();
        Mock<IGuild> guildMock = new();
        Mock<IUser> userMock = new();
        
        userMock.SetupGet(u => u.Username).Returns(username);
        messageMock.SetupGet(m => m.Author).Returns(userMock.Object);
        
        channelMock.Setup(c => c.SendMessageAsync(It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<Embed>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<AllowedMentions>(),
                It.IsAny<MessageReference>(),
                It.IsAny<MessageComponent>(),
                It.IsAny<ISticker[]>(),
                It.IsAny<Embed[]>(),
                It.IsAny<MessageFlags>(),
                It.IsAny<PollProperties>()))
            .Verifiable();
        channelMock.SetupGet(c => c.ChannelType).Returns(ChannelType.Text);
        guildMock.Setup(g => g.GetAuditLogsAsync(1, CacheMode.AllowDownload, null, null, null, ActionType.MessageDeleted, null))
            .ReturnsAsync([])
            .Verifiable();
        
        await SyncService.MessageDeleted(messageMock.Object, channelMock.Object, guildMock.Object);

        using (Assert.EnterMultipleScope())
        {
            guildMock.Verify(g => g.GetAuditLogsAsync(1, CacheMode.AllowDownload, null, null, null, ActionType.MessageDeleted, null), Times.Once);
            channelMock.Verify(c => c.SendMessageAsync(
                $"I saw that {username} :spy:",
                false,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                MessageFlags.None,
                null), Times.Once);
        }
    }

    [Test]
    public async Task AuditLogGreaterThanFiveSeconds_RespondWithAuthorUsername()
    {
        const string username = "user";
        Mock<IMessage> messageMock = new();
        Mock<IMessageChannel> channelMock = new();
        Mock<IGuild> guildMock = new();
        Mock<IAuditLogEntry> auditLogEntryMock = new();
        Mock<IUser> userMock = new();
        
        userMock.SetupGet(u => u.Username).Returns(username);
        messageMock.SetupGet(m => m.Author).Returns(userMock.Object);
        auditLogEntryMock.Setup(ale => ale.CreatedAt).Returns(DateTimeOffset.UtcNow.AddSeconds(-6));
        channelMock.Setup(c => c.SendMessageAsync(It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<Embed>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<AllowedMentions>(),
                It.IsAny<MessageReference>(),
                It.IsAny<MessageComponent>(),
                It.IsAny<ISticker[]>(),
                It.IsAny<Embed[]>(),
                It.IsAny<MessageFlags>(),
                It.IsAny<PollProperties>()))
            .Verifiable();
        channelMock.SetupGet(c => c.ChannelType).Returns(ChannelType.Text);
        guildMock.Setup(g => g.GetAuditLogsAsync(1, CacheMode.AllowDownload, null, null, null, ActionType.MessageDeleted, null))
            .ReturnsAsync([auditLogEntryMock.Object])
            .Verifiable();
        
        await SyncService.MessageDeleted(messageMock.Object, channelMock.Object, guildMock.Object);

        using (Assert.EnterMultipleScope())
        {
            guildMock.Verify(g => g.GetAuditLogsAsync(1, CacheMode.AllowDownload, null, null, null, ActionType.MessageDeleted, null), Times.Once);
            channelMock.Verify(c => c.SendMessageAsync(
                $"I saw that {username} :spy:",
                false,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                MessageFlags.None,
                null), Times.Once);
        }
    }
    
    [Test]
    public async Task AuditLogLessThanFiveSeconds_RespondWithAuditUsername()
    {
        const string username = "user";
        const string auditUsername = "audit";
        Mock<IMessage> messageMock = new();
        Mock<IMessageChannel> channelMock = new();
        Mock<IGuild> guildMock = new();
        Mock<IAuditLogEntry> auditLogEntryMock = new();
        Mock<IUser> userMock = new();
        Mock<IUser> auditUserMock = new();
        
        userMock.SetupGet(u => u.Username).Returns(username);
        auditUserMock.SetupGet(u => u.Username).Returns(auditUsername);
        messageMock.SetupGet(m => m.Author).Returns(userMock.Object);
        auditLogEntryMock.SetupGet(ale => ale.CreatedAt).Returns(DateTimeOffset.UtcNow);
        auditLogEntryMock.SetupGet(ale => ale.User).Returns(auditUserMock.Object);
        channelMock.Setup(c => c.SendMessageAsync(It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<Embed>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<AllowedMentions>(),
                It.IsAny<MessageReference>(),
                It.IsAny<MessageComponent>(),
                It.IsAny<ISticker[]>(),
                It.IsAny<Embed[]>(),
                It.IsAny<MessageFlags>(),
                It.IsAny<PollProperties>()))
            .Verifiable();
        channelMock.SetupGet(c => c.ChannelType).Returns(ChannelType.Text);
        guildMock.Setup(g => g.GetAuditLogsAsync(1, CacheMode.AllowDownload, null, null, null, ActionType.MessageDeleted, null))
            .ReturnsAsync([auditLogEntryMock.Object])
            .Verifiable();
        
        await SyncService.MessageDeleted(messageMock.Object, channelMock.Object, guildMock.Object);

        using (Assert.EnterMultipleScope())
        {
            guildMock.Verify(
                g => g.GetAuditLogsAsync(1, CacheMode.AllowDownload, null, null, null, ActionType.MessageDeleted, null),
                Times.Once);
            channelMock.Verify(c => c.SendMessageAsync(
                $"I saw that {auditUsername} :spy:",
                false,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                MessageFlags.None,
                null), Times.Once);
        }
    }

    [Test]
    public async Task ExceptionThrow_LogsException()
    {
        Mock<IMessage> messageMock = new();
        Mock<IMessageChannel> channelMock = new();
        Mock<IGuild> guildMock = new();
        channelMock.SetupGet(c => c.ChannelType).Returns(ChannelType.Text);
        
        guildMock.Setup(g => g.GetAuditLogsAsync(It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>(), It.IsAny<ulong?>(), It.IsAny<ulong?>(), It.IsAny<ActionType?>(), It.IsAny<ulong?>())).Throws<Exception>();
        
        await SyncService.MessageDeleted(messageMock.Object, channelMock.Object, guildMock.Object);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Message deleted failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }
}