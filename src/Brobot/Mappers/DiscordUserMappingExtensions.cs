using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class DiscordUserMappingExtensions
{
    public static DiscordUserResponse ToUserResponse(this DiscordUserModel model)
    {
        return new DiscordUserResponse
        {
            Id = model.Id,
            Username = model.Username,
            Birthdate = model.Birthdate,
            Timezone = model.Timezone,
            LastOnline = model.LastOnline
        };
    }
} 