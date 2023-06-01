using Brobot.Models;
using Brobot.Repositories;
using TimeZoneConverter;

namespace Brobot.Services;

public class ScheduledMessageService
{
    private readonly IUnitOfWork _uow;

    public ScheduledMessageService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ScheduledMessageModel> CreateScheduledMessage(string messageText, UserModel createdBy,
        DateTime sendDate, ChannelModel channel)
    {
        if (!string.IsNullOrWhiteSpace(createdBy.Timezone))
        {
            var timezone = TZConvert.GetTimeZoneInfo(createdBy.Timezone);
            var offset = timezone.GetUtcOffset(DateTime.Now);
            sendDate = sendDate - offset;

        }

        var reminder = new ScheduledMessageModel
        {
            MessageText = messageText,
            SendDate = sendDate,
            Channel = channel,
            ChannelId = channel.Id,
            CreatedBy = createdBy,
            CreatedById = createdBy.Id
        };

        await _uow.ScheduledMessages.Add(reminder);
        await _uow.CompleteAsync();

        return reminder;
    }
}