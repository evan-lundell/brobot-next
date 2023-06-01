namespace Brobot.Shared.Requests;

public record SyncDiscordUserRequest
{
    public required string AuthorizationCode { get; init; }
}