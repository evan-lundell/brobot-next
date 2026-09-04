using System.Security.Claims;
using Brobot.Frontend.Services;
using Brobot.Frontend.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Brobot.Frontend.Tests.Auth;

[TestFixture]
public class JwtServiceTests
{
    private static JwtService CreateService()
        => new(new ServiceCollection().BuildServiceProvider());

    [Test]
    public void Deserialize_StringClaim_IsParsed()
    {
        var token = TestJwt.Create(new Dictionary<string, object> { ["name"] = "evan" });

        var principal = CreateService().Deserialize(token);

        Assert.That(principal.FindFirst("name")?.Value, Is.EqualTo("evan"));
    }

    [Test]
    public void Deserialize_NumericClaim_IsParsedInvariantly()
    {
        var token = TestJwt.Create(new Dictionary<string, object> { ["exp"] = 1700000000 });

        var principal = CreateService().Deserialize(token);

        Assert.That(principal.FindFirst("exp")?.Value, Is.EqualTo("1700000000"));
    }

    [Test]
    public void Deserialize_ArrayClaim_ExpandsToMultipleClaims()
    {
        var token = TestJwt.Create(new Dictionary<string, object> { ["role"] = new[] { "Admin", "User" } });

        var principal = CreateService().Deserialize(token);

        var roles = principal.FindAll("role").Select(c => c.Value).ToList();
        Assert.That(roles, Is.EquivalentTo(new[] { "Admin", "User" }));
    }

    [Test]
    public void Deserialize_PayloadLengthNotMultipleOfFour_StillDecodes()
    {
        // "sub":"7" produces a payload whose base64url length needs padding restored.
        var token = TestJwt.Create(new Dictionary<string, object> { ["sub"] = "7" });

        var principal = CreateService().Deserialize(token);

        Assert.That(principal.FindFirst("sub")?.Value, Is.EqualTo("7"));
    }

    [Test]
    public void Deserialize_TokenWithoutThreeSegments_Throws()
    {
        var ex = Assert.Throws<Exception>(() => CreateService().Deserialize("only.two"));
        Assert.That(ex!.Message, Is.EqualTo("Invalid JWT"));
    }
}
