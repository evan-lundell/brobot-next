namespace Brobot.Shared.Requests;

public record StatPeriodRequest
{
    public ulong ChannelId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}