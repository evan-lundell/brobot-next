namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class DailyMessageCountModel
{
    public virtual required DiscordUserModel DiscordUser { get; init; }
    public ulong DiscordUserId { get; init; }
    
    public virtual required ChannelModel Channel { get; init; }
    public ulong ChannelId { get; init; }

    public required DateOnly CountDate { get; init; }

    public int MessageCount { get; set; }
}