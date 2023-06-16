namespace Brobot.Shared.Responses;

public class ErrorResponse
{
    public required string Type { get; init; }
    public required string Title { get; init; }
}