namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class GuildDiscordUserModel
{
    public virtual required GuildModel Guild { get; init; }
    public ulong GuildId { get; init; }

    public virtual required DiscordUserModel DiscordUser { get; init; }
    public ulong DiscordUserId { get; init; }
}