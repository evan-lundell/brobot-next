namespace Brobot.Models;

public class SecretSantaGroupUserModel
{
    public int SecretSantaGroupId { get; set; }
    public required SecretSantaGroupModel SecretSantaGroup { get; set; }

    public ulong UserId { get; set; }
    public required UserModel User { get; set; }
}