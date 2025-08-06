namespace Brobot.Shared.Responses;

public record WordCountResponse
{
    public required string Word { get; init; }
    public int Count { get; init; }
}