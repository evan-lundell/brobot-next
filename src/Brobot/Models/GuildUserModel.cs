namespace Brobot.Models;

public class GuildUserModel
{
    public virtual required GuildModel Guild { get; set; }
    public ulong GuildId { get; set; }

    public virtual required UserModel User { get; set; }
    public ulong UserId { get; set; }
}