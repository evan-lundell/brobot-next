namespace Brobot.Shared.Responses;

public class LoginResponse
{
    public bool Succeeded { get; set; }
    public string? Token { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}