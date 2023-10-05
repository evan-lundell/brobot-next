namespace Brobot.Shared.Responses;

public record SecretSantaPairResponse
{
    public required UserResponse Giver { get; init; }
    public required UserResponse Recipient { get; init; }
    public int Year { get; init; }
}