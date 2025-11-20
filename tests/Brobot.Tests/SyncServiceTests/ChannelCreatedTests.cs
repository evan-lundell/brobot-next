using Brobot.Models;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class ChannelCreatedTests : SyncServiceTestsBase
{ 
    protected override void SetupDatabase()
    {
        Context.Guilds.Add(new GuildModel
        {
            Id = 1UL,
            Name = "guild-test"
        });

        Context.Users.AddRange(new List<UserModel>
        {
            new() { Id = 3UL, Username = "user3" },
            new() { Id = 4UL, Username = "user4" }
        });
        
        Context.SaveChanges();
    }

    [Test]
    public async Task WhenChannelCreated_AddedToDatabase()
    {
        const ulong guildId = 1UL;
        const ulong channelId = 2UL;
        const string channelName = "channel-test";
        var channelMock = new Mock<IGuildChannel>();
        channelMock.Setup(c => c.GuildId).Returns(guildId);
        channelMock.Setup(c => c.Name).Returns(channelName);
        channelMock.Setup(c => c.Id).Returns(channelId);
        channelMock.Setup(c => c.GetUsersAsync(
                It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>()))
            .Returns(AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>());

        await SyncService.ChannelCreated(channelMock.Object);

        var guildModel = await Context.Guilds
            .Include(g => g.Channels)
            .SingleAsync(g => g.Id == guildId);
        var channelModel = await Context.Channels.FindAsync(channelId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(channelModel!.Name, Is.EqualTo(channelName));
            Assert.That(channelModel.Id, Is.EqualTo(channelId));
            Assert.That(guildModel.Channels, Is.Not.Empty);
            Assert.That(guildModel.Channels.First().Id, Is.EqualTo(channelId));
        }
    }
    
    [Test]
    public async Task WhenGuildNotInDatabase_NoChangesMade()
    {
        const ulong guildId = 2UL;
        const ulong channelId = 1UL;
        const string channelName = "channel-test";
        var channelMock = new Mock<IGuildChannel>();
        channelMock.Setup(c => c.GuildId).Returns(guildId);
        channelMock.Setup(c => c.Name).Returns(channelName);
        channelMock.Setup(c => c.Id).Returns(channelId);
        
        await SyncService.ChannelCreated(channelMock.Object);
        
        var channelModel = await Context.Channels.FindAsync(channelId);
        Assert.That(channelModel, Is.Null);
    }

    [Test]
    public async Task WhenChannelCreated_AddUsersToDatabase()
    {
        const ulong user1Id = 3UL;
        const ulong user2Id = 4UL;
        const ulong guildId = 1UL;
        const ulong channelId = 2UL;
        const string channelName = "channel-test";
        var mockUser1 = new Mock<IGuildUser>();
        var mockUser2 = new Mock<IGuildUser>();
        mockUser1.Setup(u => u.Id).Returns(user1Id);
        mockUser2.Setup(u => u.Id).Returns(user2Id);
        List<IGuildUser> users = [mockUser1.Object, mockUser2.Object];
        List<IReadOnlyCollection<IGuildUser>> usersList = [users];
        
        var channelMock = new Mock<IGuildChannel>();
        channelMock.Setup(c => c.GuildId).Returns(guildId);
        channelMock.Setup(c => c.Name).Returns(channelName);
        channelMock.Setup(c => c.Id).Returns(channelId);
        channelMock.Setup(c => c.GetUsersAsync(
            It.IsAny<CacheMode>(),
            It.IsAny<RequestOptions>())) 
            .Returns(usersList.ToAsyncEnumerable());
        
        await SyncService.ChannelCreated(channelMock.Object);

        var channelModel = await Context.Channels
            .Include(c => c.ChannelUsers)
            .SingleAsync(c => c.Id == channelId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel.ChannelUsers, Is.Not.Empty);
            Assert.That(channelModel.ChannelUsers.Any(u => u.UserId == user1Id), Is.True);
            Assert.That(channelModel.ChannelUsers.Any(u => u.UserId == user2Id), Is.True);
        }
    }

    [Test]
    public async Task WhenExceptionThrown_LogsError()
    {
        const ulong channelId = 1UL;
        const ulong guildId = 1UL;
        var channelMock = new Mock<IGuildChannel>();
        channelMock.Setup(c => c.Name).Throws(new Exception("Test exception"));
        channelMock.Setup(c => c.Id).Returns(channelId);
        channelMock.SetupGet(c => c.GuildId).Returns(guildId);

        await SyncService.ChannelCreated(channelMock.Object);

        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>)It.IsAny<object>())!), Times.Once);
    }
}