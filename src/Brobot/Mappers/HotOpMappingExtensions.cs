using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class HotOpMappingExtensions
{
    public static HotOpResponse ToHotOpResponse(this HotOpModel model)
    {
        return new HotOpResponse
        {
            Id = model.Id,
            DiscordUser = model.DiscordUser.ToUserResponse(),
            Channel = model.Channel.ToChannelResponse(),
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };
    }
} 