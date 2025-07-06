using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class SecretSantaGroupMappingExtensions
{
    public static SecretSantaGroupResponse ToSecretSantaGroupResponse(this SecretSantaGroupModel model)
    {
        return new SecretSantaGroupResponse
        {
            Id = model.Id,
            Name = model.Name,
            Users = model.SecretSantaGroupUsers.Select(user => user.User.ToUserResponse()).ToArray()
        };
    }
}