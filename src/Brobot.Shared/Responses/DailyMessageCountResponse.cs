namespace Brobot.Shared.Responses;

public record DailyMessageCountResponse
{
    public required DiscordUserResponse DiscordUser { get; init; }
    public DateOnly CountDate { get; init; }
    public int MessageCount { get; init; }
}