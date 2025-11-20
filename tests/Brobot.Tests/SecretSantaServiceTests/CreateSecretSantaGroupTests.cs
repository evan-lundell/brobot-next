using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Tests.SecretSantaServiceTests;

[TestFixture]
public class CreateSecretSantaGroupTests : SecretSantaServiceTestsBase
{
    [Test]
    public async Task CreatesSecretSantaGroup()
    {
        SecretSantaGroupRequest secretSantaGroup = new()
        {
            Name = "Test Group 3",
            Users = new List<UserResponse>
            {
                new()
                {
                    Id = 7,
                    Username = "User 7"
                },
                new()
                {
                    Id = 8,
                    Username = "User 8"
                }
            }
        };

        var createdGroup = await SecretSantaService.CreateSecretSantaGroup(secretSantaGroup);

        Assert.That(createdGroup.Name, Is.EqualTo(secretSantaGroup.Name));
        Assert.That(createdGroup.Users.Count, Is.EqualTo(secretSantaGroup.Users.Count));
        Assert.That(createdGroup.Id, Is.EqualTo(3));
    }
    
    [Test]
    public void ThrowsExceptionWhenUserDoesNotExist()
    {
        SecretSantaGroupRequest secretSantaGroup = new()
        {
            Name = "Test Group 3",
            Users = new List<UserResponse>
            {
                new()
                {
                    Id = 7,
                    Username = "User 7"
                },
                new()
                {
                    Id = 8,
                    Username = "User 8"
                },
                new()
                {
                    Id = 15,
                    Username = "User 15"
                }
            }
        };

        Assert.ThrowsAsync<InvalidOperationException>(async () => await SecretSantaService.CreateSecretSantaGroup(secretSantaGroup));
    }
}