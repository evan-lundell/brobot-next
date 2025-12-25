namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class SecretSantaPairModel
{
    public int Id { get; init; }
    
    public int SecretSantaGroupId { get; init; }
    public virtual required SecretSantaGroupModel SecretSantaGroup { get; init; }

    public int Year { get; init; }

    public ulong GiverDiscordUserId { get; set; }
    public virtual required DiscordUserModel GiverDiscordUser { get; set; }

    public ulong RecipientDiscordUserId { get; init; }
    public virtual required DiscordUserModel RecipientDiscordUser { get; init; }
}