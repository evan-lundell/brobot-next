using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Brobot.Models;
using Brobot.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Brobot.Services;

public class JwtService(IOptions<JwtOptions> options, ILogger<JwtService> logger) : IJwtService
{
    public string CreateJwt(ApplicationUserModel user, DiscordUserModel discordUser, string role)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            { "IdentityUserId", user.Id }
        });
        logger.LogInformation("Creating JWT");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SigningKey));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var username = discordUser.Username;

        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, username),
            new (ClaimTypes.NameIdentifier, user.Id),
            new (ClaimTypes.Email, user.Email ?? ""),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Sub, user.Id),
            new (Shared.Claims.ClaimTypes.DiscordId, discordUser.Id.ToString()),
            new (ClaimTypes.Role, role)
        };
        
        var token = new JwtSecurityToken(
            issuer: options.Value.ValidIssuer,
            audience: options.Value.ValidAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.Value.Expiry),
            signingCredentials: credentials
        );

        logger.LogInformation("JWT token created");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}