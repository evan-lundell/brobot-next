using Brobot.Configuration;
using Brobot.Models;
using Brobot.Services;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class ThreadUpdatedTests : SyncServiceTestsBase
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
        Context.SaveChanges();
    }

    [Test]
    public async Task ThreadDoesNotExistInDatabase_DoesNothing()
    {
        const ulong threadId = 2UL;
        Mock<IThreadChannel> oldThreadMock = new();
        Mock<IThreadChannel> newThreadMock = new();
        oldThreadMock.SetupGet(x => x.Id).Returns(threadId);
        newThreadMock.SetupGet(x => x.Id).Returns(threadId);
        
        await SyncService.ThreadUpdated(oldThreadMock.Object, newThreadMock.Object);
        
        Context.ChangeTracker.Clear();
        var threadModel = await Context.Channels.FindAsync(threadId);
        Assert.That(threadModel, Is.Null);
    }

    [Test]
    public async Task NameDidNotChange_DoesNothing()
    {
        const ulong threadId = 1UL;
        const string threadName = "test-thread";
        Mock<IThreadChannel> oldThreadMock = new();
        Mock<IThreadChannel> newThreadMock = new();
        oldThreadMock.SetupGet(x => x.Id).Returns(threadId);
        newThreadMock.SetupGet(x => x.Id).Returns(threadId);
        oldThreadMock.SetupGet(x => x.Name).Returns(threadName);
        newThreadMock.SetupGet(x => x.Name).Returns(threadName);
        
        await SyncService.ThreadUpdated(oldThreadMock.Object, newThreadMock.Object);
        
        Context.ChangeTracker.Clear();
        var threadModel = await Context.Channels.FindAsync(threadId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(threadModel, Is.Not.Null);
            Assert.That(threadModel!.Name, Is.EqualTo(threadName));
            Assert.That(threadModel.Archived, Is.False);
        }
    }

    [Test]
    public async Task NameUpdated_UpdatedInDatabase()
    {
        const ulong threadId = 1UL;
        const string oldThreadName = "test-thread";
        const string currentThreadName = "test-new-thread";
        Mock<IThreadChannel> oldThreadMock = new();
        Mock<IThreadChannel> newThreadMock = new();
        oldThreadMock.SetupGet(x => x.Id).Returns(threadId);
        newThreadMock.SetupGet(x => x.Id).Returns(threadId);
        oldThreadMock.SetupGet(x => x.Name).Returns(oldThreadName);
        newThreadMock.SetupGet(x => x.Name).Returns(currentThreadName);
        
        await SyncService.ThreadUpdated(oldThreadMock.Object, newThreadMock.Object);
        
        Context.ChangeTracker.Clear();
        var threadModel = await Context.Channels.FindAsync(threadId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(threadModel, Is.Not.Null);
            Assert.That(threadModel!.Name, Is.EqualTo(currentThreadName));
            Assert.That(threadModel.Archived, Is.False);
        }
    }
    
    [Test]
    public async Task ArchivedUpdated_UpdatedInDatabase()
    {
        const ulong threadId = 1UL;
        const string threadName = "test-thread";
        Mock<IThreadChannel> oldThreadMock = new();
        Mock<IThreadChannel> newThreadMock = new();
        oldThreadMock.SetupGet(x => x.Id).Returns(threadId);
        newThreadMock.SetupGet(x => x.Id).Returns(threadId);
        oldThreadMock.SetupGet(x => x.Name).Returns(threadName);
        newThreadMock.SetupGet(x => x.Name).Returns(threadName);
        oldThreadMock.SetupGet(x => x.IsArchived).Returns(false);
        newThreadMock.SetupGet(x => x.IsArchived).Returns(true);
        
        await SyncService.ThreadUpdated(oldThreadMock.Object, newThreadMock.Object);
        
        Context.ChangeTracker.Clear();
        var threadModel = await Context.Channels.FindAsync(threadId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(threadModel, Is.Not.Null);
            Assert.That(threadModel!.Name, Is.EqualTo(threadName));
            Assert.That(threadModel.Archived, Is.True);
        }
    }
    
    [Test]
    public async Task ThrowsException_LogsError()
    {
        Mock<IThreadChannel> oldThreadMock = new();
        Mock<IThreadChannel> newThreadMock = new();
        Mock<IServiceScopeFactory> serviceScopeFactoryMock = new();
        serviceScopeFactoryMock.Setup(s => s.CreateScope()).Throws<Exception>();
        SyncService syncService = new(
            serviceScopeFactoryMock.Object,
            MockDiscordClient.Object,
            LoggerMock.Object,
            Options.Create(new GeneralOptions
            {
                SeqUrl = "http://localhost:5341",
                VersionFilePath = "./version.txt"
            })
        );
        
        await syncService.ThreadUpdated(oldThreadMock.Object, newThreadMock.Object);
        
        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Thread updated failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}