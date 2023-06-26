namespace Brobot.Shared.Responses;

public record ErrorResponse
{
    public required string Type { get; init; }
    public required string Title { get; init; }
}