namespace Brobot.Shared.Responses;

public record StatPeriodResponse
{
    public int Id { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public IEnumerable<WordCountResponse> WordCounts { get; init; } = [];
    public IEnumerable<UserMessageCountResponse> UserMessageCounts { get; init; } = [];
}