using Brobot.Models;
using Brobot.Mappers;

namespace Brobot.Tests.MapperTests;

[TestFixture]
public class SecretSantaGroupMappingExtensionsTests
{
    [Test]
    public void ToSecretSantaGroupResponse_MapsModelToResponse()
    {
        var userModel = new UserModel { Id = 1, Username = "user1" };
        var secretSantaGroupModel = new SecretSantaGroupModel
        {
            Id = 1,
            Name = "secret"
        };
        var groupUser = new SecretSantaGroupUserModel
        {
            User = userModel,
            SecretSantaGroup = secretSantaGroupModel
        };
        var model = new SecretSantaGroupModel
        {
            Id = 11,
            Name = "GroupA",
            SecretSantaGroupUsers = new List<SecretSantaGroupUserModel> { groupUser }
        };
        var response = model.ToSecretSantaGroupResponse();
        var responseUsers = response.Users.ToArray();
        Assert.That(response.Id, Is.EqualTo(11));
        Assert.That(response.Name, Is.EqualTo("GroupA"));
        Assert.That(responseUsers.Length, Is.EqualTo(1));
        Assert.That(responseUsers[0].Id, Is.EqualTo(1));
    }
} 