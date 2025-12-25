namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class ChannelDiscordUserModel
{
    public virtual required ChannelModel Channel { get; init; }
    public ulong ChannelId { get; init; }

    public virtual required DiscordUserModel DiscordUser { get; init; }
    public ulong UserId { get; init; }
}