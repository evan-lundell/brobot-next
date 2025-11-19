using Brobot.Configuration;
using Brobot.Models;
using Brobot.Services;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    
    [Test]
    public async Task ThrowsException_LogsError()
    {
        Mock<IThreadChannel> threadChannelMock = new();
        Mock<IServiceScopeFactory> serviceScopeFactoryMock = new();
        serviceScopeFactoryMock.Setup(ssf => ssf.CreateScope()).Throws<Exception>();
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
        
        await syncService.ThreadDeleted(threadChannelMock.Object);

        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Thread deleted failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}    
