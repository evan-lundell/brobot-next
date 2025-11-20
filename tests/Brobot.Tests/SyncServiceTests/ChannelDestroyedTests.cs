using Brobot.Configuration;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class ChannelDestroyedTests : SyncServiceTestsBase
{
    protected override void SetupDatabase()
    {
        GuildModel guild = new()
        {
            Id = 1UL,
            Name = "test-guild",
        };

        ChannelModel channel1 = new()
        {
            Id = 1UL,
            Name = "test-channel1",
            Guild = guild,
            GuildId = guild.Id
        };

        ChannelModel channel2 = new()
        {
            Id = 2UL,
            Name = "test-channel2",
            Guild = guild,
            GuildId = guild.Id
        };
        
        Context.Channels.AddRange(channel1, channel2);
        Context.Guilds.Add(guild);
        Context.SaveChanges();
    }

    [Test]
    public async Task WhenChannelDestroyed_ShouldDeleteChannel()
    {
        const ulong channelId = 1UL;
        Mock<IGuildChannel> channelMock = new();
        channelMock.Setup(x => x.Id).Returns(channelId);
        
        await SyncService.ChannelDestroyed(channelMock.Object);

        Context.ChangeTracker.Clear();
        var channelModel = await Context.Channels.FindAsync(channelId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel, Is.Not.Null);
            Assert.That(channelModel!.Archived, Is.True);
        }
    }

    [Test]
    public async Task WhenChannelIsNotInDatabase_NoChangesToDatabase()
    {
        const ulong existingChannelId1 = 1UL;
        const ulong existingChannelId2 = 2UL;
        const ulong mockedChannelId = 3UL;
        Mock<IGuildChannel> channelMock = new();
        channelMock.Setup(x => x.Id).Returns(mockedChannelId);
        
        await SyncService.ChannelDestroyed(channelMock.Object);
        
        Context.ChangeTracker.Clear();
        var channelModel1 = await Context.Channels.FindAsync(existingChannelId1);
        var channelModel2 = await Context.Channels.FindAsync(existingChannelId2);
        var channelModel3 = await Context.Channels.FindAsync(mockedChannelId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModel1, Is.Not.Null);
            Assert.That(channelModel2, Is.Not.Null);
            Assert.That(channelModel3, Is.Null);
            Assert.That(Context.Channels.Count, Is.EqualTo(2));
            Assert.That(channelModel1!.Archived, Is.False);
            Assert.That(channelModel2!.Archived, Is.False);
        }
    }

    [Test]
    public async Task WhenExceptionOccurs_LogError()
    {
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider.GetService(typeof(IUnitOfWork)))
            .Throws(new Exception("Database error"));
        
        var syncServiceWithFaultyUow = new SyncService(
            scopeFactoryMock.Object,
            MockDiscordClient.Object,
            LoggerMock.Object,
            Options.Create(new GeneralOptions
            {
                SeqUrl = "http://localhost:5341",
                VersionFilePath = "./version.txt"
            }));
        
        await syncServiceWithFaultyUow.ChannelDestroyed(Mock.Of<IGuildChannel>());
        
        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>)It.IsAny<object>())!), Times.Once);
    }
}