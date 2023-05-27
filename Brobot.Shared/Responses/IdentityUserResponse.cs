namespace Brobot.Shared.Responses;

public class IdentityUserResponse
{
    public required string Id { get; init; }
    public string? Email { get; init; }
    public string? Username { get; init; }
    public bool IsDiscordAuthenticated { get; set; }
}