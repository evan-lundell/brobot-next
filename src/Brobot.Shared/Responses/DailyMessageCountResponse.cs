namespace Brobot.Shared.Responses;

public record DailyMessageCountResponse
{
    public required UserResponse User { get; init; }
    public DateOnly CountDate { get; init; }
    public int MessageCount { get; init; }
}