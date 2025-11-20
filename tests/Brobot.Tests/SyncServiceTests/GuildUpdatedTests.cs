using Brobot.Configuration;
using Brobot.Models;
using Brobot.Services;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class GuildUpdatedTests : SyncServiceTestsBase
{
    protected override void SetupDatabase()
    {
        GuildModel guild = new()
        {
            Id = 1UL,
            Name = "test-guild"
        };
        Context.Guilds.Add(guild);
        Context.SaveChanges();
    }

    [Test]
    public async Task NamesAreTheSame_NoUpdate()
    {
        const string guildName = "test-guild";
        const ulong guildId = 1UL;
        Mock<IGuild> previousGuildMock = new();
        Mock<IGuild> currentGuildMock = new();
        previousGuildMock.Setup(x => x.Name).Returns(guildName);
        currentGuildMock.Setup(x => x.Name).Returns(guildName);
        
        await SyncService.GuildUpdated(previousGuildMock.Object, currentGuildMock.Object);
        
        Context.ChangeTracker.Clear();
        var guildModel = await Context.Guilds.FindAsync(guildId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.Name, Is.EqualTo(guildName));
        }
    }
    
    [Test]
    public async Task GuildNotInDatabase_NoUpdate()
    {
        const ulong existingGuildId = 1UL;
        const string guildName = "test-guild";
        const string previousGuildName = "test-guild";
        const string currentGuildName = "updated-guild";
        const  ulong mockedGuildId = 2UL;
        Mock<IGuild> previousGuildMock = new();
        Mock<IGuild> currentGuildMock = new();
        previousGuildMock.Setup(x => x.Name).Returns(previousGuildName);
        currentGuildMock.Setup(x => x.Name).Returns(currentGuildName);
        previousGuildMock.Setup(x => x.Id).Returns(mockedGuildId);
        currentGuildMock.Setup(x => x.Id).Returns(mockedGuildId);
        
        await SyncService.GuildUpdated(previousGuildMock.Object, currentGuildMock.Object);
        
        Context.ChangeTracker.Clear();
        var guildModel1 = await Context.Guilds.FindAsync(existingGuildId);
        var guildModel2 = await Context.Guilds.FindAsync(mockedGuildId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildModel1, Is.Not.Null);
            Assert.That(guildModel1!.Name, Is.EqualTo(guildName));
            Assert.That(guildModel2, Is.Null);
        }
    }
    
    [Test]
    public async Task NamesAreDifferent_UpdateInDatabase()
    {
        const string previousGuildName = "test-guild";
        const string currentGuildName = "updated-guild";
        const ulong guildId = 1UL;
        Mock<IGuild> previousGuildMock = new();
        Mock<IGuild> currentGuildMock = new();
        previousGuildMock.Setup(x => x.Name).Returns(previousGuildName);
        currentGuildMock.Setup(x => x.Name).Returns(currentGuildName);
        previousGuildMock.Setup(x => x.Id).Returns(guildId);
        currentGuildMock.Setup(x => x.Id).Returns(guildId);
        
        await SyncService.GuildUpdated(previousGuildMock.Object, currentGuildMock.Object);
        
        Context.ChangeTracker.Clear();
        var guildModel = await Context.Guilds.FindAsync(guildId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.Name, Is.EqualTo(currentGuildName));
        }
    }

    [Test]
    public async Task ThrowsException_LogsError()
    {
        Mock<IGuild> previousGuildMock = new();
        Mock<IGuild> currentGuildMock = new();
        Mock<IServiceScopeFactory> serviceScopeFactoryMock = new();
        previousGuildMock.SetupGet(p => p.Name).Returns("previous");
        currentGuildMock.SetupGet(p => p.Name).Returns("current");
        serviceScopeFactoryMock.Setup(s => s.CreateScope())
            .Throws<Exception>();
        SyncService syncServiceWithFaultyScopeFactory = new(
            serviceScopeFactoryMock.Object,
            MockDiscordClient.Object,
            LoggerMock.Object,
            Options.Create(new GeneralOptions
            {
                SeqUrl = "http://localhost:5341",
                VersionFilePath = "./version.txt"
            })
        );
        
        await syncServiceWithFaultyScopeFactory.GuildUpdated(previousGuildMock.Object, currentGuildMock.Object);
        
        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing guild unavailable for GuildId")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}