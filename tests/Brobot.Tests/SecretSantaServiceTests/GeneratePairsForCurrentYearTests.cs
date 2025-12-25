using Brobot.Exceptions;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Tests.SecretSantaServiceTests;

[TestFixture]
public class GeneratePairsForCurrentYearTests : SecretSantaServiceTestsBase
{
    [Test]
    public void ThrowsExceptionWhenGroupDoesNotExist()
    {
        Assert.ThrowsAsync<ModelNotFoundException<SecretSantaGroupModel, int>>(() => SecretSantaService.GeneratePairsForCurrentYear(3));
    }

    [Test]
    public void ThrowsExceptionWhenGroupHasCurrentYearPairings()
    {
        Assert.ThrowsAsync<InvalidOperationException>(() => SecretSantaService.GeneratePairsForCurrentYear(2));
    }

    [Test]
    public async Task GeneratesValidPairs()
    {
        var secretSantaGroupModel = await Context.SecretSantaGroups
            .Include(ssg => ssg.SecretSantaPairs)
            .ThenInclude(ssp => ssp.GiverDiscordUser)
            .Include(ssg => ssg.SecretSantaPairs)
            .ThenInclude(ssp => ssp.RecipientDiscordUser)
            .Include(ssg => ssg.SecretSantaGroupUsers)
            .ThenInclude(ssgu => ssgu.DiscordUser)
            .SingleAsync(ssg => ssg.Id == 1);
        
        var pairs = await SecretSantaService.GeneratePairsForCurrentYear(1);
        
        var giverIds = new HashSet<ulong>();
        var recipientIds = new HashSet<ulong>();
        foreach (var pair in pairs)
        {
            Assert.That(giverIds.Contains(pair.Giver.Id), Is.False);
            Assert.That(recipientIds.Contains(pair.Recipient.Id), Is.False);
            var lastYearPair = secretSantaGroupModel.SecretSantaPairs.FirstOrDefault((ssp) =>
                ssp.Year == DateTime.UtcNow.AddYears(-1).Year && ssp.GiverDiscordUserId == pair.Giver.Id && ssp.RecipientDiscordUserId == pair.Recipient.Id);
            Assert.That(lastYearPair, Is.Null);
            giverIds.Add(pair.Giver.Id);
            recipientIds.Add(pair.Recipient.Id);
        }
    }
}