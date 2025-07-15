using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Brobot.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Brobot.Services;

public class JwtService(IConfiguration configuration) : IJwtService
{
    public string CreateJwt(IdentityUser user, UserModel? discordUser, string? role = null)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSigningKey"] ?? ""));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var username = discordUser?.Username ?? user.UserName ?? "";

        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, username),
            new (ClaimTypes.NameIdentifier, user.Id),
            new (ClaimTypes.Email, user.Email ?? ""),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Sub, user.Id)
        };

        if (discordUser?.Id != null)
        {
            claims.Add(new Claim(Shared.Claims.ClaimTypes.DiscordId, discordUser.Id.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (!int.TryParse(configuration["JwtExpiry"], out var expiry))
        {
            expiry = 30;
        }
        var token = new JwtSecurityToken(
            issuer: configuration["ValidIssuer"] ?? "",
            audience: configuration["ValidAudience"] ?? "",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}