namespace Brobot.Shared.Responses;

public record SecretSantaGroupResponse
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required ICollection<UserResponse> Users { get; init; }
}