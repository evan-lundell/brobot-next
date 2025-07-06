using Microsoft.AspNetCore.Identity;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class IdentityUserMappingExtensions
{
    public static IdentityUserResponse ToIdentityUserResponse(this IdentityUser user)
    {
        return new IdentityUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.UserName,
            // IsDiscordAuthenticated will be set elsewhere
        };
    }
}