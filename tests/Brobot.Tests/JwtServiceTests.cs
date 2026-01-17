using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Brobot.Configuration;
using Brobot.Models;
using Brobot.Services;
using Brobot.Shared;
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
        var applicationUserId = new Guid().ToString();
        var discordUserId = 1UL;
        var discordUsername = "Discord User1";
        DiscordUserModel discordUser = new()
        {
            Id = discordUserId,
            Archived = false,
            Username = discordUsername
        };
        
        var applicationUserMock = new Mock<ApplicationUserModel>();
        applicationUserMock.SetupGet(u => u.UserName).Returns(discordUsername);
        applicationUserMock.SetupGet(u => u.Id).Returns(applicationUserId);
        applicationUserMock.SetupGet(u => u.Email).Returns("test1@test.com");

        var token = _jwtService.CreateJwt(applicationUserMock.Object, discordUser, [Constants.UserRoleName]);

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
            Assert.That(jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value, Is.EqualTo(applicationUserId));
            Assert.That(jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value, Is.EqualTo("test1@test.com"));
            Assert.That(jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value, Is.EqualTo(Constants.UserRoleName));
        });
    }
} 