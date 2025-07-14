using Brobot.Models;
using Discord;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class ThreadDeletedTests : SyncServiceTestsBase
{
    protected override void SetupDatabase()
    {
        GuildModel guild = new()
        {
            Id = 1UL,
            Name = "test-guild"
        };
        Context.Guilds.Add(guild);
        ChannelModel threadChannelModel = new()
        {
            Id = 1UL,
            Name = "test-thread",
            GuildId = guild.Id,
            Guild = guild
        };
        Context.Channels.Add(threadChannelModel);
        Context.SaveChanges();
    }

    [Test]
    public async Task ThreadNotInDatabase_DoNothing()
    {
        const ulong channelId = 2UL;
        Mock<IThreadChannel> threadChannelMock = new();
        threadChannelMock.SetupGet(t => t.Id).Returns(channelId);

        await SyncService.ThreadDeleted(threadChannelMock.Object);

        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels.FindAsync(channelId);
        Assert.That(channelModel, Is.Null);
    }

    [Test]
    public async Task ThreadInDatabase_ArchivesChannel()
    {
        const ulong channelId = 1UL;
        Mock<IThreadChannel> threadChannelMock = new();
        threadChannelMock.SetupGet(t => t.Id).Returns(channelId);

        await SyncService.ThreadDeleted(threadChannelMock.Object);

        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels.FindAsync(channelId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(channelModel!.Archived, Is.True);
        }
    }
}    
