using Brobot.Models;

namespace Brobot.Dtos;

public record AuthResultDto(
    bool Succeeded,
    ApplicationUserModel? User = null,
    DiscordUserModel? DiscordUser = null,
    IList<string>? Roles = null,
    string? ErrorMessage = null);
    