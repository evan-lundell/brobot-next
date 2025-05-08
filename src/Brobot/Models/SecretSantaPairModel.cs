namespace Brobot.Models;

public class SecretSantaPairModel
{
    public int Id { get; set; }
    
    public int SecretSantaGroupId { get; set; }
    public virtual required SecretSantaGroupModel SecretSantaGroup { get; set; }

    public int Year { get; set; }

    public ulong GiverUserId { get; set; }
    public virtual required UserModel GiverUser { get; set; }

    public ulong RecipientUserId { get; set; }
    public virtual required UserModel RecipientUser { get; set; }
}