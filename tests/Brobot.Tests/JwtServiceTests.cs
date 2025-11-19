using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Brobot.Configuration;
using Brobot.Models;
using Brobot.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Brobot.Tests;

[TestFixture]
public class JwtServiceTests
{
    private JwtService _jwtService;
    private const string SigningKey = "ov3b8UFUIUZsUq6pH9ErVfYQfrGVJ3WakuSdqZhcgOo";
    private const string Issuer = "brobot";
    private const string Audience = "brobot";

    [SetUp]
    public void Setup()
    {
        JwtOptions options = new()
        {
            SigningKey = SigningKey,
            ValidAudience = Audience,
            ValidIssuer = Issuer,
            Expiry = 30
        };
        
        _jwtService = new JwtService(Options.Create(options), Mock.Of<ILogger<JwtService>>());
    }

    [Test]
    public void CreateJwt_ValidInput_ReturnsValidToken()
    {
        var identityUserId = new Guid().ToString();
        var identityUserMock = new Mock<IdentityUser>();
        identityUserMock.SetupGet(u => u.UserName).Returns("Identity User1");
        identityUserMock.SetupGet(u => u.Id).Returns(identityUserId);
        identityUserMock.SetupGet(u => u.Email).Returns("test1@test.com");
        
        UserModel discordUser = new()
        {
            Id = 1UL,
            Archived = false,
            Username = "Discord User1",
            IdentityUserId = identityUserId
        };

        var token = _jwtService.CreateJwt(identityUserMock.Object, discordUser, "user");

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token);
        var jwtToken = jsonToken as JwtSecurityToken;
        if (jwtToken == null)
        {
            throw new Exception("JWT is null");
        }
        
        Assert.Multiple(() =>
        {
            Assert.That(jwtToken.Issuer, Is.EqualTo(Issuer));
            Assert.That(jwtToken.Audiences.First(), Is.EqualTo(Audience));
            Assert.That(jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value, Is.EqualTo("Discord User1"));
            Assert.That(jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value, Is.EqualTo(identityUserId));
            Assert.That(jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value, Is.EqualTo("test1@test.com"));
        });
    }
} 