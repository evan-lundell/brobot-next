namespace Brobot.Shared.Responses;

public record ApplicationUserResponse
{
    public required string Id { get; init; }
    public string? Email { get; init; }
    public string? Username { get; init; }
}
