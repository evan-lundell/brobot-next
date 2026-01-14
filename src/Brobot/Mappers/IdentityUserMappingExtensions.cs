using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class IdentityUserMappingExtensions
{
    public static ApplicationUserResponse ToApplicationUserResponse(this ApplicationUserModel user)
    {
        return new ApplicationUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.UserName
        };
    }
}