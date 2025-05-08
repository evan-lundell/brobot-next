namespace Brobot.Models;

public class SecretSantaGroupUserModel
{
    public int SecretSantaGroupId { get; set; }
    public virtual required SecretSantaGroupModel SecretSantaGroup { get; set; }

    public ulong UserId { get; set; }
    public virtual required UserModel User { get; set; }
}