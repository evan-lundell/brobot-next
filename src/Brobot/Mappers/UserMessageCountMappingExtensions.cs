using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class UserMessageCountMappingExtensions
{
    public static UserMessageCountResponse ToUserMessageCountResponse(this DiscordUserMessageCountModel model)
    {
        return new UserMessageCountResponse
        {
            UserId = model.DiscordUserId,
            Count = model.Count
        };
    }
}