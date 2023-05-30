namespace Brobot.Dtos;

public class ScoreboardDto
{
    public int HotOpId { get; init; }
    public required string OwnerUsername { get; init; }
    public required IEnumerable<ScoreboardItemDto> Scores { get; init; }
}