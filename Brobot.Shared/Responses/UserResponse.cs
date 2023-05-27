namespace Brobot.Shared.Responses;

public class UserResponse
{
    public ulong Id { get; init; }
    public required string Username { get; init; }
    public DateTime? Birthdate { get; init; }
    public string? Timezone { get; init; }
    public DateTime? LastOnline { get; init; }
}