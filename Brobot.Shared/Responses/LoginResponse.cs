namespace Brobot.Shared.Responses;

public class LoginResponse
{
    public bool Succeeded { get; init; }
    public string? Token { get; init; }
    public IEnumerable<string>? Errors { get; set; }
}