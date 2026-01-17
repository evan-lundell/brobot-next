namespace Brobot.Shared.Responses;

public record SecretSantaPairResponse
{
    public required DiscordUserResponse Giver { get; init; }
    public required DiscordUserResponse Recipient { get; init; }
    public int Year { get; init; }
}