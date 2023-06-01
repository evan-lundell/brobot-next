namespace Brobot.Shared.Responses;

public record DiscordAuthResponse
{
    public required string Url { get; init; }
}