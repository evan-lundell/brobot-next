namespace Brobot.Dtos;

public class ScoreboardDto
{
    public int HotOpId { get; set; }
    public required string OwnerUsername { get; set; }
    public required IEnumerable<ScoreboardItemDto> Scores { get; set; }
}