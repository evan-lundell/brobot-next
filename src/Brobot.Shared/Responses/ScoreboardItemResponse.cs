namespace Brobot.Shared.Responses;

public record ScoreboardItemResponse
{
    public ulong UserId { get; init; }
    public required string Username { get; init; }
    public int Score { get; set; }
}