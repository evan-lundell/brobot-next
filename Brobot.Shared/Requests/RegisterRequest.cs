namespace Brobot.Shared.Requests;

public class RegisterRequest
{
    public required string EmailAddress { get; set; }
    public required string DisplayName { get; set; }
    public required string Password { get; set; }
}