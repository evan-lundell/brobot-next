namespace Brobot.Shared.Responses;

public record DiscordUserResponse
{
    public ulong Id { get; init; }
    public required string Username { get; init; }
    public DateOnly? Birthdate { get; init; }
    public string? Timezone { get; init; }
    public DateTimeOffset? LastOnline { get; init; }
}