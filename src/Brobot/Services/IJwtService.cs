using Brobot.Models;
using Microsoft.AspNetCore.Identity;

namespace Brobot.Services;

public interface IJwtService
{
    string CreateJwt(IdentityUser user, DiscordUserModel? discordUser, string? role = null);
}