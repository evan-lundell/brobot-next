using Brobot.Models;
using Discord;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class GuildUnavailableTests : SyncServiceTestsBase
{
    protected override void SetupDatabase()
    {
        GuildModel guild = new()
        {
            Id = 1UL,
            Name = "guild-test"
        };
        Context.Guilds.Add(guild);
        Context.SaveChanges();
    }

    [Test]
    public async Task GuildUnavailable_MarkedAsArchived()
    {
        const ulong guildId = 1UL;
        Mock<IGuild>  guildMock = new();
        guildMock.SetupGet(g => g.Id).Returns(guildId);
        
        await SyncService.GuildUnavailable(guildMock.Object);
        
        Context.ChangeTracker.Clear();
        var guildModel =  await Context.Guilds.FindAsync(guildId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.Archived, Is.True);
        }
    }

    [Test]
    public async Task GuildNotInDatabase_NoChangesMade()
    {
        const ulong existingGuildId = 1UL;
        const ulong mockedGuildId = 2UL;
        Mock<IGuild> guildMock = new();
        guildMock.SetupGet(g => g.Id).Returns(mockedGuildId);
        
        await SyncService.GuildUnavailable(guildMock.Object);
        
        Context.ChangeTracker.Clear();
        var guildModel1 = await Context.Guilds.FindAsync(existingGuildId);
        var guildModel2 = await Context.Guilds.FindAsync(mockedGuildId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildModel1, Is.Not.Null);
            Assert.That(guildModel1!.Archived, Is.False);
            Assert.That(guildModel2, Is.Null);
        }
    }
}