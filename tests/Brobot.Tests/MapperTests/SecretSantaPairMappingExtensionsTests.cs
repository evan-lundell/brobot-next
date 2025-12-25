using Brobot.Models;
using Brobot.Mappers;

namespace Brobot.Tests.MapperTests;

[TestFixture]
public class SecretSantaPairMappingExtensionsTests
{
    [Test]
    public void ToSecretSantaPairResponse_MapsModelToResponse()
    {
        var giver = new DiscordUserModel { Id = 1, Username = "giver" };
        var recipient = new DiscordUserModel { Id = 2, Username = "recipient" };
        var secretSantaGroup = new SecretSantaGroupModel
        {
            Id = 1,
            Name = "secret-santa-group"
        };
        var model = new SecretSantaPairModel
        {
            GiverDiscordUser = giver,
            RecipientDiscordUser = recipient,
            Year = 2023,
            SecretSantaGroup = secretSantaGroup
        };
        var response = model.ToSecretSantaPairResponse();
        Assert.That(response.Giver.Id, Is.EqualTo(1));
        Assert.That(response.Recipient.Id, Is.EqualTo(2));
        Assert.That(response.Year, Is.EqualTo(2023));
    }
} 