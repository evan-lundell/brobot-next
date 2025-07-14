using Brobot.Models;
using Discord;
using Discord.WebSocket;
using Moq;

namespace Brobot.Tests.SyncServiceTests;


public class ChannelUpdatedTest : SyncServiceTestsBase
{
    protected override void SetupDatabase()
    {
        GuildModel guild = new()
        {
            Name = "test-guild1",
            Id = 1UL
        };

        ChannelModel channel = new()
        {
            Id = 1UL,
            Name = "test-channel1",
            Guild = guild,
            GuildId = guild.Id
        };

        Context.Channels.Add(channel);
        Context.Guilds.Add(guild);
        Context.SaveChanges();
    }

    [Test]
    public async Task NameDidntChange_NoChanges()
    {
        const string channelName = "test-channel1";
        const ulong channelId = 1UL;
        Mock<IGuild> guildMock = new();
        Mock<ISocketMessageChannel> previousChannelMock = new();
        Mock<ISocketMessageChannel> currentChannelMock = new();
        previousChannelMock.Setup(x => x.Name).Returns(channelName);
        previousChannelMock.Setup(x => x.Id).Returns(channelId);
        currentChannelMock.Setup(x => x.Name).Returns(channelName);
        currentChannelMock.Setup(x => x.Id).Returns(channelId);
        
        await SyncService.ChannelUpdated(guildMock.Object, previousChannelMock.Object, currentChannelMock.Object);

        var channelModel = await Context.Channels.FindAsync(channelId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(channelModel!.Name, Is.EqualTo(channelName));
            Assert.That(guildMock.Invocations, Is.Empty);
        }
    }

    [Test]
    public async Task NameChanged_AuditLogFound_SendsMessage()
    {
        const string username = "test-user1";
        const string previousChannelName = "test-channel1";
        const string currentChannelName = "updated-channel1";
        const ulong channelId = 1UL;
        Mock<IGuild> guildMock = new();
        Mock<ISocketMessageChannel> previousChannelMock = new();
        Mock<ISocketMessageChannel> currentChannelMock = new();
        Mock<IAuditLogEntry> auditLogEntryMock = new();
        Mock<IUser> userMock = new();
        userMock.Setup(u => u.Username).Returns(username);
        auditLogEntryMock.Setup(ale => ale.User).Returns(userMock.Object);
        List<IAuditLogEntry> auditLogs = [auditLogEntryMock.Object];
        
        guildMock.Setup(x => x.GetAuditLogsAsync(
                1,
                CacheMode.AllowDownload,
                null,
                null,
                null,
                ActionType.ChannelUpdated,
                null))
            .ReturnsAsync(auditLogs);
        previousChannelMock.Setup(x => x.Name).Returns(previousChannelName);
        previousChannelMock.Setup(x => x.Id).Returns(channelId);
        currentChannelMock.Setup(x => x.Name).Returns(currentChannelName);
        currentChannelMock.Setup(x => x.Id).Returns(channelId);
        
        await SyncService.ChannelUpdated(guildMock.Object, previousChannelMock.Object, currentChannelMock.Object);

        using (Assert.EnterMultipleScope())
        {
            guildMock.Verify(x => x.GetAuditLogsAsync(
                1,
                CacheMode.AllowDownload,
                null,
                null,
                null,
                ActionType.ChannelUpdated,
                null), Times.Once);
            currentChannelMock.Verify(x => x.SendMessageAsync(
                $"{username} changed the channel name from '{previousChannelName}' to '{currentChannelName}'",
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
    public async Task NameChanged_AuditLogNotFound_SendsMessage()
    {
        const string username = "test-user1";
        const string previousChannelName = "test-channel1";
        const string currentChannelName = "updated-channel1";
        const ulong channelId = 1UL;
        Mock<IGuild> guildMock = new();
        Mock<ISocketMessageChannel> previousChannelMock = new();
        Mock<ISocketMessageChannel> currentChannelMock = new();
        Mock<IAuditLogEntry> auditLogEntryMock = new();
        Mock<IUser> userMock = new();
        userMock.Setup(u => u.Username).Returns(username);
        auditLogEntryMock.Setup(ale => ale.User).Returns(userMock.Object);
        
        guildMock.Setup(x => x.GetAuditLogsAsync(
                1,
                CacheMode.AllowDownload,
                null,
                null,
                null,
                ActionType.ChannelUpdated,
                null))
            .ReturnsAsync([]);
        previousChannelMock.Setup(x => x.Name).Returns(previousChannelName);
        previousChannelMock.Setup(x => x.Id).Returns(channelId);
        currentChannelMock.Setup(x => x.Name).Returns(currentChannelName);
        currentChannelMock.Setup(x => x.Id).Returns(channelId);
        
        await SyncService.ChannelUpdated(guildMock.Object, previousChannelMock.Object, currentChannelMock.Object);

        using (Assert.EnterMultipleScope())
        {
            guildMock.Verify(x => x.GetAuditLogsAsync(
                1,
                CacheMode.AllowDownload,
                null,
                null,
                null,
                ActionType.ChannelUpdated,
                null), Times.Once);
            currentChannelMock.Verify(x => x.SendMessageAsync(
                $"Channel name changed from '{previousChannelName}' to '{currentChannelName}'",
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
    public async Task NameChanged_DatabaseUpdated()
    {
        const string username = "test-user1";
        const string previousChannelName = "test-channel1";
        const string currentChannelName = "updated-channel1";
        const ulong channelId = 1UL;
        Mock<IGuild> guildMock = new();
        Mock<ISocketMessageChannel> previousChannelMock = new();
        Mock<ISocketMessageChannel> currentChannelMock = new();
        Mock<IAuditLogEntry> auditLogEntryMock = new();
        Mock<IUser> userMock = new();
        userMock.Setup(u => u.Username).Returns(username);
        auditLogEntryMock.Setup(ale => ale.User).Returns(userMock.Object);
        
        guildMock.Setup(x => x.GetAuditLogsAsync(
                1,
                CacheMode.AllowDownload,
                null,
                null,
                null,
                ActionType.ChannelUpdated,
                null))
            .ReturnsAsync([]);
        previousChannelMock.Setup(x => x.Name).Returns(previousChannelName);
        previousChannelMock.Setup(x => x.Id).Returns(channelId);
        currentChannelMock.Setup(x => x.Name).Returns(currentChannelName);
        currentChannelMock.Setup(x => x.Id).Returns(channelId);
        
        await SyncService.ChannelUpdated(guildMock.Object, previousChannelMock.Object, currentChannelMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels.FindAsync(channelId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(channelModel!.Name, Is.EqualTo(currentChannelName));
        }
    }
    
    [Test]
    public async Task NameChanged_ChannelNotInDatabase_DoNotUpdate()
    {
        const string username = "test-user1";
        const string previousChannelName = "test-channel1";
        const string currentChannelName = "updated-channel1";
        const ulong channelId = 3UL;
        Mock<IGuild> guildMock = new();
        Mock<ISocketMessageChannel> previousChannelMock = new();
        Mock<ISocketMessageChannel> currentChannelMock = new();
        Mock<IAuditLogEntry> auditLogEntryMock = new();
        Mock<IUser> userMock = new();
        userMock.Setup(u => u.Username).Returns(username);
        auditLogEntryMock.Setup(ale => ale.User).Returns(userMock.Object);
        
        guildMock.Setup(x => x.GetAuditLogsAsync(
                1,
                CacheMode.AllowDownload,
                null,
                null,
                null,
                ActionType.ChannelUpdated,
                null))
            .ReturnsAsync([]);
        previousChannelMock.Setup(x => x.Name).Returns(previousChannelName);
        previousChannelMock.Setup(x => x.Id).Returns(channelId);
        currentChannelMock.Setup(x => x.Name).Returns(currentChannelName);
        currentChannelMock.Setup(x => x.Id).Returns(channelId);
        
        await SyncService.ChannelUpdated(guildMock.Object, previousChannelMock.Object, currentChannelMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels.FindAsync(channelId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Null);
        }
    }
}