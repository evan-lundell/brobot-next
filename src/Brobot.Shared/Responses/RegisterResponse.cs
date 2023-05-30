namespace Brobot.Shared.Responses;

public class RegisterResponse
{
    public bool Succeeded { get; init; }
    public IEnumerable<string>? Errors { get; init; }
}