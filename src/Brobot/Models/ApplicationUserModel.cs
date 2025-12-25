using Microsoft.AspNetCore.Identity;

namespace Brobot.Models;

public class ApplicationUserModel : IdentityUser
{
    public ulong DiscordUserId { get; set; }
    public required DiscordUserModel DiscordUser { get; set; }
}