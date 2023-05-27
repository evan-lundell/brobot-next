namespace Brobot.Models;

public class DailyMessageCountModel
{
    public required UserModel User { get; set; }
    public ulong UserId { get; set; }

    public required DateOnly CountDate { get; set; }

    public int MessageCount { get; set; }
}