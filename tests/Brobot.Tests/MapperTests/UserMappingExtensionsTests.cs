using Brobot.Models;
using Brobot.Mappers;

namespace Brobot.Tests.MapperTests;

[TestFixture]
public class UserMappingExtensionsTests
{
    [Test]
    public void ToUserResponse_MapsModelToResponse()
    {
        var now = DateTimeOffset.UtcNow;
        var model = new UserModel
        {
            Id = 7,
            Username = "testuser",
            Birthdate = new DateOnly(2000, 1, 1),
            Timezone = "UTC",
            LastOnline = now
        };
        var response = model.ToUserResponse();
        Assert.That(response.Id, Is.EqualTo(7));
        Assert.That(response.Username, Is.EqualTo("testuser"));
        Assert.That(response.Birthdate, Is.EqualTo(new DateOnly(2000, 1, 1)));
        Assert.That(response.Timezone, Is.EqualTo("UTC"));
        Assert.That(response.LastOnline, Is.EqualTo(now));
    }
}
