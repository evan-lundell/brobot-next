using Brobot.Models;
using Discord;
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
}