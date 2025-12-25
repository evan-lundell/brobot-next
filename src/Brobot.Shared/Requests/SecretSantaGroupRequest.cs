using Brobot.Shared.Responses;

namespace Brobot.Shared.Requests;

public record SecretSantaGroupRequest
{
    public required string Name { get; set; }
    public required ICollection<UserResponse> Users { get; init; }
}