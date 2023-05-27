using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Brobot.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Brobot.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string CreateJwt(IdentityUser user, UserModel? discordUser, string? role = null, ulong? userId = null)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSigningKey"] ?? ""));
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

        if (userId.HasValue)
        {
            claims.Add(new Claim(Brobot.Shared.Claims.ClaimTypes.DiscordId, userId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (!int.TryParse(_configuration["JwtExpiry"], out var expiry))
        {
            expiry = 30;
        }
        var token = new JwtSecurityToken(
            issuer: _configuration["ValidIssuer"] ?? "",
            audience: _configuration["ValidAudience"] ?? "",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}