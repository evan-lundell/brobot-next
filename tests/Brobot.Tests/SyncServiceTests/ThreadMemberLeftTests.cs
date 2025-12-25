using Brobot.Configuration;
using Brobot.Models;
using Brobot.Services;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class ThreadMemberLeftTests : SyncServiceTestsBase
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

        DiscordUserModel discordUser = new()
        {
            Id = 1UL,
            Username = "test-user"
        };
        Context.DiscordUsers.Add(discordUser);

        DiscordUserModel user2 = new()
        {
            Id = 2UL,
            Username = "test-user2"
        };
        Context.DiscordUsers.Add(user2);
        thread.ChannelUsers.Add(new ChannelDiscordUserModel
        {
            ChannelId = thread.Id,
            Channel = thread,
            UserId = discordUser.Id,
            DiscordUser = discordUser
        });
        Context.SaveChanges();
    }

    [Test]
    public async Task UserIsBot_DoesNothing()
    {
        const ulong channelId = 1UL;
        Mock<IThreadUser> threadUserMock = new();
        Mock<IGuildUser> guildUserMock = new();
        guildUserMock.Setup(u => u.IsBot).Returns(true);
        threadUserMock.SetupGet(u => u.GuildUser).Returns(guildUserMock.Object);
        
        await SyncService.ThreadMemberLeft(threadUserMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels
            .Include(c => c.ChannelUsers)
            .SingleOrDefaultAsync(c => c.Id == channelId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(channelModel!.ChannelUsers, Has.Count.EqualTo(1));
        }
    }
    
    [Test]
    public async Task ChannelDoesNotExist_DoesNothing()
    {
        const ulong channelId = 2UL;
        const ulong userId = 1UL;
        Mock<IThreadUser> threadUserMock = new();
        Mock<IGuildUser> guildUserMock = new();
        Mock<IThreadChannel> threadChannelMock = new();
        guildUserMock.SetupGet(u => u.Id).Returns(userId);
        threadUserMock.SetupGet(u => u.GuildUser).Returns(guildUserMock.Object);
        threadChannelMock.SetupGet(t => t.Id).Returns(channelId);
        threadUserMock.SetupGet(t => t.Thread).Returns(threadChannelMock.Object);
        
        await SyncService.ThreadMemberLeft(threadUserMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels.FindAsync(channelId);
        var userModel = await Context.DiscordUsers
            .Include(u => u.ChannelUsers)
            .SingleOrDefaultAsync(u => u.Id == userId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Null);
            Assert.That(userModel, Is.Not.Null);
            Assert.That(userModel!.ChannelUsers, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task UserDoesNotExist_DoesNothing()
    {
        const ulong channelId = 1UL;
        const ulong userId = 3UL;
        Mock<IThreadUser> threadUserMock = new();
        Mock<IGuildUser> guildUserMock = new();
        Mock<IThreadChannel> threadChannelMock = new();
        guildUserMock.SetupGet(u => u.Id).Returns(userId);
        threadUserMock.SetupGet(u => u.GuildUser).Returns(guildUserMock.Object);
        threadChannelMock.SetupGet(t => t.Id).Returns(channelId);
        threadUserMock.SetupGet(t => t.Thread).Returns(threadChannelMock.Object);
        
        await SyncService.ThreadMemberLeft(threadUserMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels
            .Include(c => c.ChannelUsers)
            .SingleOrDefaultAsync(c => c.Id == channelId);
        var userModel = await Context.DiscordUsers.FindAsync(userId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(userModel, Is.Null);
            Assert.That(channelModel!.ChannelUsers, Has.Count.EqualTo(1));
        }
    }
    
    [Test]
    public async Task ChannelUserDoesNotExist_DoesNothing()
    {
        const ulong channelId = 1UL;
        const ulong userId = 2UL;
        Mock<IThreadUser> threadUserMock = new();
        Mock<IGuildUser> guildUserMock = new();
        Mock<IThreadChannel> threadChannelMock = new();
        guildUserMock.SetupGet(u => u.Id).Returns(userId);
        threadUserMock.SetupGet(u => u.GuildUser).Returns(guildUserMock.Object);
        threadChannelMock.SetupGet(t => t.Id).Returns(channelId);
        threadUserMock.SetupGet(t => t.Thread).Returns(threadChannelMock.Object);
        
        await SyncService.ThreadMemberLeft(threadUserMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels
            .Include(c => c.ChannelUsers)
            .SingleOrDefaultAsync(c => c.Id == channelId);
        var userModel = await Context.DiscordUsers.FindAsync(userId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(userModel, Is.Not.Null);
            Assert.That(channelModel!.ChannelUsers, Has.Count.EqualTo(1));
            Assert.That(channelModel.ChannelUsers.FirstOrDefault(cu => cu.UserId == userId), Is.Null);
        }
    }

    [Test]
    public async Task UserIsInThread_RemovesUser()
    {
        const ulong channelId = 1UL;
        const ulong userId = 1UL;
        Mock<IThreadUser> threadUserMock = new();
        Mock<IGuildUser> guildUserMock = new();
        Mock<IThreadChannel> threadChannelMock = new();
        guildUserMock.SetupGet(u => u.Id).Returns(userId);
        threadUserMock.SetupGet(u => u.GuildUser).Returns(guildUserMock.Object);
        threadChannelMock.SetupGet(t => t.Id).Returns(channelId);
        threadUserMock.SetupGet(t => t.Thread).Returns(threadChannelMock.Object);
        
        await SyncService.ThreadMemberLeft(threadUserMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels
            .Include(c => c.ChannelUsers)
            .SingleOrDefaultAsync(c => c.Id == channelId);
        var userModel = await Context.DiscordUsers
            .Include(u => u.ChannelUsers)
            .SingleOrDefaultAsync(u => u.Id == userId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(userModel, Is.Not.Null);
            Assert.That(channelModel!.ChannelUsers, Has.Count.EqualTo(0));
            Assert.That(userModel!.ChannelUsers, Has.Count.EqualTo(0));
        }
    }
    
    [Test]
    public async Task ThrowsException_LogsError()
    {
        Mock<IThreadUser> threadUserMock = new();
        Mock<IGuildUser> guildUserMock = new();
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        threadUserMock.SetupGet(s => s.GuildUser).Returns(guildUserMock.Object);
        guildUserMock.SetupGet(gu => gu.IsBot).Returns(false);
        scopeFactoryMock.Setup(sf => sf.CreateScope()).Throws<Exception>();
        SyncService syncService = new(
            scopeFactoryMock.Object,
            MockDiscordClient.Object,
            LoggerMock.Object,
            Options.Create(new GeneralOptions
            {
                SeqUrl = "http://localhost:5341",
                VersionFilePath = "./version.txt"
            })
        );
        
        await syncService.ThreadMemberLeft(threadUserMock.Object);
        
        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Thread member left failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }
}