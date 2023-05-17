namespace Brobot.Shared.Responses;

public class RegisterResponse
{
    public bool Succeeded { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}