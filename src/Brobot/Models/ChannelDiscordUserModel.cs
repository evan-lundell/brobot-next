namespace Brobot.Models;

public class ChannelDiscordUserModel
{
    public virtual required ChannelModel Channel { get; set; }
    public ulong ChannelId { get; set; }

    public virtual required DiscordUserModel DiscordUser { get; set; }
    public ulong UserId { get; set; }
}