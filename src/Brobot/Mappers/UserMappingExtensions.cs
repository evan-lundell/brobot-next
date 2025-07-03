using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class UserMappingExtensions
{
    public static UserResponse ToUserResponse(this UserModel model)
    {
        return new UserResponse
        {
            Id = model.Id,
            Username = model.Username,
            Birthdate = model.Birthdate,
            Timezone = model.Timezone,
            LastOnline = model.LastOnline
        };
    }
} 