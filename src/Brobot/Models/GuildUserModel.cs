namespace Brobot.Models;

public class GuildUserModel
{
    public required GuildModel Guild { get; set; }
    public ulong GuildId { get; set; }

    public required UserModel User { get; set; }
    public ulong UserId { get; set; }
}