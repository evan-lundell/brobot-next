using Brobot.Models;
using Microsoft.AspNetCore.Identity;

namespace Brobot.Services;

public interface IJwtService
{
    string CreateJwt(ApplicationUserModel user, DiscordUserModel discordUser, IEnumerable<string> roles);
}