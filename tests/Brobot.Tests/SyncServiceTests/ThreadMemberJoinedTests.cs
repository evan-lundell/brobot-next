using Brobot.Models;
using Discord;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class ThreadMemberJoinedTests : SyncServiceTestsBase
{
    protected override void SetupDatabase()
    {
        GuildModel guild = new()
        {
            Id = 1UL,
            Name = "test-guild"
        };
        Context.Guilds.Add(guild);

        ChannelModel thread = new()
        {
            Id = 1UL,
            Name = "test-thread",
            GuildId = guild.Id,
            Guild = guild
        };
        Context.Channels.Add(thread);

        UserModel user1 = new()
        {
            Id = 1UL,
            Username = "existing-user"
        };
        Context.Users.Add(user1);
        
        guild.GuildUsers.Add(new GuildUserModel
        {
            GuildId = guild.Id,
            Guild = guild,
            UserId = user1.Id,
            User = user1
        });
        
        thread.ChannelUsers.Add(new ChannelUserModel
        {
            ChannelId = thread.Id,
            Channel = thread,
            UserId = user1.Id,
            User = user1
        });

        UserModel user2 = new()
        {
            Id = 2UL,
            Username = "new-user"
        };
        Context.Users.Add(user2);
        
        Context.SaveChanges();
    }

    [Test]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public async Task UserIsBotOrWebhook_DoesNothing(bool isBot, bool isWebhook)
    {
        const ulong userId = 2UL;
        const ulong threadId = 1UL;
        Mock<IGuildUser> guildUserMock = new();
        Mock<IThreadUser> threadUserMock = new();
        guildUserMock.SetupGet(u => u.Id).Returns(userId);
        guildUserMock.SetupGet(u => u.IsBot).Returns(isBot);
        guildUserMock.SetupGet(u => u.IsWebhook).Returns(isWebhook);
        threadUserMock.SetupGet(u => u.GuildUser).Returns(guildUserMock.Object);
        
        await SyncService.ThreadMemberJoined(threadUserMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels
            .Include(c => c.ChannelUsers)
            .Where(u => u.ChannelUsers.Count > 0)
            .SingleOrDefaultAsync(c => c.Id == threadId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(channelModel!.ChannelUsers, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task ChannelDoesNotExist_DoesNothing()
    {
        const ulong userId = 2UL;
        const ulong threadId = 2UL;
        Mock<IGuildUser> guildUserMock = new();
        Mock<IThreadUser> threadUserMock = new();
        Mock<IThreadChannel> threadChannelMock = new();
        guildUserMock.SetupGet(u => u.Id).Returns(userId);
        threadUserMock.SetupGet(u => u.GuildUser).Returns(guildUserMock.Object);
        threadChannelMock.SetupGet(t => t.Id).Returns(threadId);
        threadUserMock.SetupGet(t => t.Thread).Returns(threadChannelMock.Object);
        
        await SyncService.ThreadMemberJoined(threadUserMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels
            .Include(c => c.ChannelUsers)
            .SingleOrDefaultAsync(c => c.Id == threadId);
        var channelUsersCount = await Context.Users
            .Include(u => u.ChannelUsers)
            .Where(u => u.ChannelUsers.Count > 0)
            .CountAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Null);
            Assert.That(channelUsersCount, Is.EqualTo(1));
        }
    }
    
    [Test]
    public async Task UserDoesNotExist_DoesNothing()
    {
        const ulong userId = 3UL;
        const ulong threadId = 1UL;
        Mock<IGuildUser> guildUserMock = new();
        Mock<IThreadUser> threadUserMock = new();
        Mock<IThreadChannel> threadChannelMock = new();
        guildUserMock.SetupGet(u => u.Id).Returns(userId);
        threadUserMock.SetupGet(u => u.GuildUser).Returns(guildUserMock.Object);
        threadChannelMock.SetupGet(t => t.Id).Returns(threadId);
        threadUserMock.SetupGet(t => t.Thread).Returns(threadChannelMock.Object);
        
        await SyncService.ThreadMemberJoined(threadUserMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels
            .Include(c => c.ChannelUsers)
            .SingleOrDefaultAsync(c => c.Id == threadId);
        var channelUsersCount = await Context.Users
            .Include(u => u.ChannelUsers)
            .Where(u => u.ChannelUsers.Count > 0)
            .CountAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(channelModel!.ChannelUsers, Has.Count.EqualTo(1));
            Assert.That(channelUsersCount, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task UserIsValid_UserIsAddedToChannel()
    {
        const ulong userId = 2UL;
        const ulong threadId = 1UL;
        Mock<IGuildUser> guildUserMock = new();
        Mock<IThreadUser> threadUserMock = new();
        Mock<IThreadChannel> threadChannelMock = new();
        guildUserMock.SetupGet(u => u.Id).Returns(userId);
        threadUserMock.SetupGet(u => u.GuildUser).Returns(guildUserMock.Object);
        threadChannelMock.SetupGet(t => t.Id).Returns(threadId);
        threadUserMock.SetupGet(t => t.Thread).Returns(threadChannelMock.Object);
        
        await SyncService.ThreadMemberJoined(threadUserMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels
            .Include(c => c.ChannelUsers)
            .SingleOrDefaultAsync(c => c.Id == threadId);
        var channelUsersCount = await Context.Users
            .Include(u => u.ChannelUsers)
            .CountAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(channelModel!.ChannelUsers, Has.Count.EqualTo(2));
            Assert.That(channelUsersCount, Is.EqualTo(2));
            Assert.That(channelModel.ChannelUsers.FirstOrDefault(cu => cu.UserId == userId), Is.Not.Null);
        }
    }
}