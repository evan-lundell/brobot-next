using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class ChannelMappingExtensions
{
    public static ChannelResponse ToChannelResponse(this ChannelModel model)
    {
        return new ChannelResponse
        {
            Id = model.Id,
            Name = model.Name
        };
    }
} 