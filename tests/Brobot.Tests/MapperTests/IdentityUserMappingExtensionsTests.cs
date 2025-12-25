using Brobot.Mappers;
using Brobot.Models;
using Microsoft.AspNetCore.Identity;

namespace Brobot.Tests.MapperTests;

[TestFixture]
public class IdentityUserMappingExtensionsTests
{
    [Test]
    public void ToIdentityUserResponse_MapsIdentityUserToResponse()
    {
        var discordUserId = 1UL;
        var discordUserName = "testuser";
        var discordUser = new DiscordUserModel
        {
            Id = discordUserId,
            Username = discordUserName
        };
        var user = new ApplicationUserModel
        {
            Id = "abc",
            Email = "test@example.com",
            UserName = discordUserName,
            DiscordUserId = 1UL,
            DiscordUser = discordUser
        };
        var response = user.ToApplicationUserResponse();
        Assert.That(response.Id, Is.EqualTo("abc"));
        Assert.That(response.Email, Is.EqualTo("test@example.com"));
        Assert.That(response.Username, Is.EqualTo("testuser"));
    }
}
