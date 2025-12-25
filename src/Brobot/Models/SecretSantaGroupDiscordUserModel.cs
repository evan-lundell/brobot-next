namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class SecretSantaGroupDiscordUserModel
{
    public int SecretSantaGroupId { get; init; }
    public virtual required SecretSantaGroupModel SecretSantaGroup { get; init; }

    public ulong DiscordUserId { get; init; }
    public virtual required DiscordUserModel DiscordUser { get; init; }
}