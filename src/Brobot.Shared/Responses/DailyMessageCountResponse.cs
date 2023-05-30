namespace Brobot.Shared.Responses;

public class DailyMessageCountResponse
{
    public required UserResponse User { get; init; }
    public DateOnly CountDate { get; init; }
    public int MessageCount { get; init; }
}