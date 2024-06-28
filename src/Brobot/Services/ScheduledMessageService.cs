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
    
    public async Task<IEnumerable<ScheduledMessageModel>> GetScheduledMessagesByUser(UserModel user, int limit, int skip,
        DateTime? scheduledBefore, DateTime? scheduledAfter)
    {
        var scheduledMessages = (await _uow.ScheduledMessages.GetScheduledMessagesByUser(user.Id, limit, skip, scheduledBefore, scheduledAfter)).ToList();
        foreach (var message in scheduledMessages)
        {
            if (string.IsNullOrWhiteSpace(message.CreatedBy.Timezone))
            {
                continue;
            }

            var timezone = TZConvert.GetTimeZoneInfo(message.CreatedBy.Timezone);
            var offset = timezone.GetUtcOffset(DateTime.UtcNow);
            if (message.SendDate.HasValue)
            {
                message.SendDate = message.SendDate.Value.ToOffset(offset);
            }

            if (message.SentDate.HasValue)
            {
                message.SentDate = message.SentDate.Value.ToOffset(offset);
            }
        }

        return scheduledMessages;
    }

    public async Task<ScheduledMessageModel> CreateScheduledMessage(string messageText, UserModel createdBy,
        DateTime sendDate, ulong channelId)
    {
        var offset = TimeSpan.FromHours(0);
        var sendDateUnspecified = new DateTime(sendDate.Ticks, DateTimeKind.Unspecified);
        if (!string.IsNullOrWhiteSpace(createdBy.Timezone))
        {
            var timezone = TZConvert.GetTimeZoneInfo(createdBy.Timezone);
            offset = timezone.GetUtcOffset(DateTime.Now);
        }
        var sendDateAdjusted = new DateTimeOffset(sendDateUnspecified, offset).ToUniversalTime();
        
        if (sendDateAdjusted < DateTimeOffset.UtcNow)
        {
            throw new Exception("Send date cannot be in the past");
        }
        
        var channelModel = await _uow.Channels.GetById(channelId);

        if (channelModel == null)
        {
            throw new Exception("Channel not found");
        }
        
        var reminder = new ScheduledMessageModel
        {
            MessageText = messageText,
            SendDate = sendDateAdjusted,
            Channel = channelModel,
            ChannelId = channelId,
            CreatedBy = createdBy,
            CreatedById = createdBy.Id
        };

        await _uow.ScheduledMessages.Add(reminder);
        await _uow.CompleteAsync();

        return reminder;
    }
    
    public async Task<ScheduledMessageModel?> UpdateScheduledMessage(int id, string? text = null, ulong? channelId = null, DateTime? sendDate = null)
    {
        var scheduledMessage = await _uow.ScheduledMessages.GetById(id);
        if (scheduledMessage == null)
        {
            return null;
        }

        if (scheduledMessage.SentDate.HasValue)
        {
            throw new Exception("Cannot update a sent message");
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            scheduledMessage.MessageText = text;
        }

        if (channelId.HasValue)
        {
            var channel = await _uow.Channels.GetById(channelId.Value);
            if (channel == null)
            {
                throw new Exception("Channel not found");
            }
            scheduledMessage.ChannelId = channelId.Value;
        }

        var user = scheduledMessage.CreatedBy;
        var offset = TimeSpan.FromHours(0);
        if (!string.IsNullOrEmpty(user.Timezone))
        {
            var tz = TZConvert.GetTimeZoneInfo(user.Timezone);
            offset = tz.GetUtcOffset(DateTime.UtcNow);
        }
        if (sendDate.HasValue)
        {
            var newSendDate = new DateTimeOffset(sendDate.Value, offset).ToUniversalTime();
            if (newSendDate < DateTimeOffset.UtcNow)
            {
                throw new Exception("Send date cannot be in the past");
            }
            scheduledMessage.SendDate = newSendDate;
        }
        
        await _uow.CompleteAsync();
        
        scheduledMessage.SendDate = scheduledMessage.SendDate?.ToOffset(offset);
        return scheduledMessage;
    }
    
    public async Task<bool> DeleteScheduledMessage(int id)
    {
        var scheduledMessage = await _uow.ScheduledMessages.GetById(id);
        if (scheduledMessage == null)
        {
            return false;
        }

        _uow.ScheduledMessages.Remove(scheduledMessage);
        await _uow.CompleteAsync();
        return true;
    }
    
    public async Task<bool> CanUserUpdateScheduledMessage(UserModel user, int scheduledMessageId)
    {
        var scheduledMessage = await _uow.ScheduledMessages.GetById(scheduledMessageId);
        return scheduledMessage != null && scheduledMessage.CreatedById == user.Id;
    }
}