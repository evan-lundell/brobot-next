namespace Brobot.Shared.Responses;

public record PlaylistSongResponse
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required string Artist { get; init; }
    public int Order { get; init; }
}