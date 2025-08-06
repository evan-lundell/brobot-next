using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class UserMessageCountMappingExtensions
{
    public static UserMessageCountResponse ToUserMessageCountResponse(this UserMessageCountModel model)
    {
        return new UserMessageCountResponse
        {
            UserId = model.UserId,
            Count = model.Count
        };
    }
}