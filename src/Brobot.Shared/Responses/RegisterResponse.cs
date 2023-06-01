namespace Brobot.Shared.Responses;

public record RegisterResponse
{
    public bool Succeeded { get; init; }
    public IEnumerable<string>? Errors { get; init; }
}