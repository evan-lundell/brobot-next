namespace Brobot.Models;

public class DiscordUserMessageCountModel
{
    public int Id { get; init; }
    public ulong DiscordUserId { get; init; }
    public int Count { get; init; }

    public required StatPeriodModel StatPeriod { get; init; }
    public int StatPeriodId { get; init; }
}