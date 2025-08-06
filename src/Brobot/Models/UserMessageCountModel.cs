namespace Brobot.Models;

public class UserMessageCountModel
{
    public int Id { get; set; }
    public ulong UserId { get; set; }
    public int Count { get; set; }

    public required StatPeriodModel StatPeriod { get; set; }
    public int StatPeriodId { get; set; }
}