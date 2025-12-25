namespace Brobot.Models;

public class WordCountModel
{
    public int Id { get; init; }
    public required string Word { get; init; }
    public int  Count { get; init; }

    public required StatPeriodModel StatPeriod { get; init; }
    public int StatPeriodId { get; init; }
}