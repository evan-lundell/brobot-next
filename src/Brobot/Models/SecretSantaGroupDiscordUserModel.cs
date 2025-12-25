namespace Brobot.Models;

public class SecretSantaGroupDiscordUserModel
{
    public int SecretSantaGroupId { get; set; }
    public virtual required SecretSantaGroupModel SecretSantaGroup { get; set; }

    public ulong DiscordUserId { get; set; }
    public virtual required DiscordUserModel DiscordUser { get; set; }
}