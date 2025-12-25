namespace Brobot.Models;

public class GuildDiscordUserModel
{
    public virtual required GuildModel Guild { get; set; }
    public ulong GuildId { get; set; }

    public virtual required DiscordUserModel DiscordUser { get; set; }
    public ulong DiscordUserId { get; set; }
}