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

public class ThreadCreatedTests : SyncServiceTestsBase
{
    protected override void SetupDatabase()
    {
        GuildModel guildModel = new()
        {
            Id = 1UL,
            Name = "test-guild"
        };
        Context.Guilds.Add(guildModel);
        ChannelModel threadChannelModel = new()
        {
            Id = 1UL,
            Name = "test-thread",
            GuildId = guildModel.Id,
            Guild = guildModel,
            Archived = false
        };
        Context.Channels.Add(threadChannelModel);
        ChannelModel archivedThreadChannelModel = new()
        {
            Id = 2UL,
            Name = "test-archived-thread",
            GuildId = guildModel.Id,
            Guild = guildModel,
            Archived = true
        };
        Context.Channels.Add(archivedThreadChannelModel);
        UserModel user = new()
        {
            Id = 1UL,
            Username = "test-user"
        };
        Context.Users.Add(user);
        guildModel.GuildUsers.Add(new  GuildUserModel
        {
            Guild = guildModel,
            GuildId = guildModel.Id,
            User = user,
            UserId =  user.Id
        });
        Context.SaveChanges();
    }

    [Test]
    public async Task GuildNotInDatabase_DoesNothing()
    {
        const ulong guildId = 2UL;
        Mock<IThreadChannel> threadChannelMock = new();
        Mock<IGuild> guildMock = new();
        guildMock.Setup(g => g.Id).Returns(guildId);
        threadChannelMock.Setup(t => t.Guild).Returns(guildMock.Object);
        
        await SyncService.ThreadCreated(threadChannelMock.Object);

        var channelCount = await Context.Channels.CountAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelCount, Is.EqualTo(2));
            LoggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Thread creation failed. Guild")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }
    }
    
    [Test]
    public async Task ThreadAlreadyExists_DoesNothing()
    {
        const ulong guildId = 1UL;
        const ulong channelId = 1UL;
        Mock<IThreadChannel> threadChannelMock = new();
        Mock<IGuild> guildMock = new();
        guildMock.Setup(g => g.Id).Returns(guildId);
        threadChannelMock.Setup(t => t.Guild).Returns(guildMock.Object);
        threadChannelMock.SetupGet(t => t.Id).Returns(channelId);
        
        await SyncService.ThreadCreated(threadChannelMock.Object);

        var channelCount = await Context.Channels.CountAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelCount, Is.EqualTo(2));
            LoggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Thread creation failed. Thread with ID")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }
    }

    [Test]
    public async Task ThreadExistsAndIsArchived_SetArchivedToFalse()
    {
        const ulong guildId = 1UL;
        const ulong channelId = 2UL;
        Mock<IThreadChannel> threadChannelMock = new();
        Mock<IGuild> guildMock = new();
        guildMock.Setup(g => g.Id).Returns(guildId);
        threadChannelMock.Setup(t => t.Guild).Returns(guildMock.Object);
        threadChannelMock.SetupGet(t => t.Id).Returns(channelId);
        
        await SyncService.ThreadCreated(threadChannelMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel =  await Context.Channels.FindAsync(channelId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(channelModel!.Archived, Is.False);
        }
    }

    [Test]
    public async Task ThreadDoesNotExist_ChannelIsCreated()
    {
        const ulong guildId = 1UL;
        const ulong  channelId = 3UL;
        const ulong userId = 1UL;
        const string threadName = "new-thread";
        Mock<IThreadChannel> threadChannelMock = new();
        Mock<IGuild> guildMock = new();
        guildMock.Setup(g => g.Id).Returns(guildId);
        threadChannelMock.Setup(t => t.Guild).Returns(guildMock.Object);
        threadChannelMock.SetupGet(t => t.Id).Returns(channelId);
        threadChannelMock.SetupGet(t => t.Name).Returns(threadName);
        threadChannelMock.SetupGet(t => t.OwnerId).Returns(userId);
        
        await SyncService.ThreadCreated(threadChannelMock.Object);
        
        Context.ChangeTracker.Clear();
        var newThreadModel = await Context.Channels
            .Include(c => c.ChannelUsers)
            .SingleAsync(c => c.Id == channelId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(newThreadModel, Is.Not.Null);
            Assert.That(newThreadModel.GuildId, Is.EqualTo(guildId));
            Assert.That(newThreadModel.Name, Is.EqualTo(threadName));
            Assert.That(newThreadModel.ChannelUsers, Has.Count.EqualTo(1));
            Assert.That(newThreadModel.ChannelUsers.First().UserId, Is.EqualTo(userId));
        }
    }

    [Test]
    public async Task ExceptionThrown_LogsException()
    {
        Mock<IServiceScopeFactory> serviceScopeFactoryMock = new();
        serviceScopeFactoryMock.Setup(s => s.CreateScope()).Throws<Exception>();
        Mock<IThreadChannel> threadChannelMock = new();
        threadChannelMock.SetupGet(t => t.Id).Returns(1UL);
        
        SyncService syncService = new(
            serviceScopeFactoryMock.Object,
            MockDiscordClient.Object,
            LoggerMock.Object,
            Options.Create(new GeneralOptions
            {
                SeqUrl = "http://localhost:5341",
                VersionFilePath = "./version.txt"
            }));

        await syncService.ThreadCreated(threadChannelMock.Object);
        
        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Thread creation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }
}