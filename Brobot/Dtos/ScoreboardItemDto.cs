namespace Brobot.Dtos;

public class ScoreboardItemDto
{
    public ulong UserId { get; set; }
    public required string Username { get; set; }
    public int Score { get; set; }
}