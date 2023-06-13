namespace Brobot.Shared.Responses;

public record PlaylistResponse
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required IEnumerable<PlaylistSongResponse> Songs { get; init; }
}