namespace Brobot.Models;

public class DiscordUserMessageCountModel
{
    public int Id { get; set; }
    public ulong DiscordUserId { get; set; }
    public int Count { get; set; }

    public required StatPeriodModel StatPeriod { get; set; }
    public int StatPeriodId { get; set; }
}