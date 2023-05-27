namespace Brobot.Dtos;

public class ScoreboardItemDto
{
    public ulong UserId { get; init; }
    public required string Username { get; init; }
    public int Score { get; set; }
}