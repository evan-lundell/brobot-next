using Brobot.Mappers;
using Microsoft.AspNetCore.Identity;

namespace Brobot.Tests.MapperTests;

[TestFixture]
public class IdentityUserMappingExtensionsTests
{
    [Test]
    public void ToIdentityUserResponse_MapsIdentityUserToResponse()
    {
        var user = new IdentityUser { Id = "abc", Email = "test@example.com", UserName = "testuser" };
        var response = user.ToIdentityUserResponse();
        Assert.That(response.Id, Is.EqualTo("abc"));
        Assert.That(response.Email, Is.EqualTo("test@example.com"));
        Assert.That(response.Username, Is.EqualTo("testuser"));
    }
}
