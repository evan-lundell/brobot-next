using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Brobot.Models;
using Brobot.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Brobot.Tests;

[TestFixture]
public class JwtServiceTests
{
    private JwtService _jwtService;
    private readonly string _signingKey = "ov3b8UFUIUZsUq6pH9ErVfYQfrGVJ3WakuSdqZhcgOo";
    private readonly string _issuer = "brobot";
    private readonly string _audience = "brobot";

    [SetUp]
    public void Setup()
    {
        Dictionary<string, string?> settings = new()
        {
            { "JwtSigningKey", _signingKey },
            { "ValidIssuer", _issuer },
            { "ValidAudience", _audience }
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        _jwtService = new JwtService(configuration);
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
            Assert.That(jwtToken.Issuer, Is.EqualTo(_issuer));
            Assert.That(jwtToken.Audiences.First(), Is.EqualTo(_audience));
            Assert.That(jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value, Is.EqualTo("Discord User1"));
            Assert.That(jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value, Is.EqualTo(identityUserId));
            Assert.That(jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value, Is.EqualTo("test1@test.com"));
        });
    }
} 