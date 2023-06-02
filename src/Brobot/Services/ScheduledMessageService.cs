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
        var offset = TimeSpan.FromHours(0);
        if (!string.IsNullOrWhiteSpace(createdBy.Timezone))
        {
            var timezone = TZConvert.GetTimeZoneInfo(createdBy.Timezone);
            offset = timezone.GetUtcOffset(DateTime.Now);
        }
        var sendDateAdjusted = new DateTimeOffset(sendDate, offset).ToUniversalTime();

        var reminder = new ScheduledMessageModel
        {
            MessageText = messageText,
            SendDate = sendDateAdjusted,
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