using Brobot.Models;

namespace Brobot.Services;

public interface IJwtService
{
    string CreateJwt(ApplicationUserModel user, DiscordUserModel discordUser, IEnumerable<string> roles);
}