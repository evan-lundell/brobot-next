namespace Brobot.Models;

public class WordCountModel
{
    public int Id { get; set; }
    public required string Word { get; set; }
    public int  Count { get; set; }

    public required StatPeriodModel StatPeriod { get; set; }
    public int StatPeriodId { get; set; }
}