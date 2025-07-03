using Brobot.Models;
using Brobot.Shared.Responses;

namespace Brobot.Mappers;

public static class ScheduledMessageMappingExtensions
{
    public static ScheduledMessageResponse ToScheduledMessageResponse(this ScheduledMessageModel model)
    {
        return new ScheduledMessageResponse
        {
            Id = model.Id,
            MessageText = model.MessageText,
            SendDate = model.SendDate,
            SentDate = model.SentDate,
            Channel = model.Channel.ToChannelResponse(),
            CreatedById = model.CreatedById
        };
    }
} 