namespace Brobot.Shared.Responses;

public class ScoreboardResponse
{
    public int HotOpId { get; init; }
    public required string OwnerUsername { get; init; }
    public required IEnumerable<ScoreboardItemResponse> Scores { get; init; }
}