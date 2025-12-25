namespace Brobot.Models;

public class SecretSantaPairModel
{
    public int Id { get; set; }
    
    public int SecretSantaGroupId { get; set; }
    public virtual required SecretSantaGroupModel SecretSantaGroup { get; set; }

    public int Year { get; set; }

    public ulong GiverDiscordUserId { get; set; }
    public virtual required DiscordUserModel GiverDiscordUser { get; set; }

    public ulong RecipientDiscordUserId { get; set; }
    public virtual required DiscordUserModel RecipientDiscordUser { get; set; }
}