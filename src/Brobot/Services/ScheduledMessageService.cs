using Brobot.Exceptions;
using Brobot.Models;
using Brobot.Repositories;
using TimeZoneConverter;

namespace Brobot.Services;

public class ScheduledMessageService(IUnitOfWork uow, ILogger<ScheduledMessageService> logger) : IScheduledMessageService
{
    public async Task<IEnumerable<ScheduledMessageModel>> GetScheduledMessagesByUser(DiscordUserModel discordUser, int? limit = null, int skip = 0,
        DateTime? scheduledBefore = null, DateTime? scheduledAfter = null)
    {
        logger.LogInformation(
            "Getting scheduled messages for user {UserId}. Limit: {Limit}, Skip: {Skip}, ScheduledBefore: {ScheduledBefore}, ScheduledAfter: {ScheduledAfter}",
            discordUser.Id,
            limit,
            skip,
            scheduledBefore, 
            scheduledAfter
        );
        var scheduledMessages = (await uow.ScheduledMessages.GetScheduledMessagesByUser(discordUser.Id, limit, skip, scheduledBefore, scheduledAfter)).ToList();
        if (!string.IsNullOrWhiteSpace(discordUser.Timezone))
        {

            foreach (var message in scheduledMessages)
            {
                if (message.SendDate.HasValue)
                {
                    message.SendDate = message.SendDate.Value.AdjustToUsersTimezone(discordUser.Timezone);
                }

                if (message.SentDate.HasValue)
                {
                    message.SentDate = message.SentDate.Value.AdjustToUsersTimezone(discordUser.Timezone);
                }
            }
        }

        logger.LogInformation(
            "Finished getting scheduled messages for user {UserId}. Limit: {Limit}, Skip: {Skip}, ScheduledBefore: {ScheduledBefore}, ScheduledAfter: {ScheduledAfter}",
            discordUser.Id,
            limit,
            skip,
            scheduledBefore, 
            scheduledAfter
        );
        return scheduledMessages;
    }

    public async Task<ScheduledMessageModel> CreateScheduledMessage(string messageText, DiscordUserModel createdBy,
        DateTime sendDate, ulong channelId)
    {
        logger.LogInformation("Creating a scheduled message for user {UserId}, channel {ChannelId}, with send date of {SendDate}", createdBy.Id,  channelId, sendDate);
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
            throw new InvalidOperationException("Send date cannot be in the past");
        }
        
        var channelModel = await uow.Channels.GetById(channelId);

        if (channelModel == null)
        {
            throw new InvalidOperationException("Channel not found");
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

        await uow.ScheduledMessages.Add(reminder);
        await uow.CompleteAsync();

        logger.LogInformation("Finished creating a scheduled message for user {UserId}, channel {ChannelId}, with send date of {SendDate}", createdBy.Id,  channelId, sendDate);
        return reminder;
    }
    
    public async Task<ScheduledMessageModel> UpdateScheduledMessage(int id, string? text = null, ulong? channelId = null, DateTime? sendDate = null)
    {
        logger.LogInformation("Updating scheduled message {ScheduledMessageId}", id);
        var scheduledMessage = await uow.ScheduledMessages.GetById(id);
        if (scheduledMessage == null)
        {
            throw new ModelNotFoundException<ScheduledMessageModel, int>(id);
        }

        if (scheduledMessage.SentDate.HasValue)
        {
            throw new InvalidOperationException("Cannot update a sent message");
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            scheduledMessage.MessageText = text;
        }

        if (channelId.HasValue)
        {
            var channel = await uow.Channels.GetById(channelId.Value);
            if (channel == null)
            {
                throw new InvalidOperationException("Channel not found");
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
                throw new InvalidOperationException("Send date cannot be in the past");
            }
            scheduledMessage.SendDate = newSendDate;
        }
        
        await uow.CompleteAsync();
        scheduledMessage.SendDate = scheduledMessage.SendDate?.ToOffset(offset);
        
        logger.LogInformation("Finished updating scheduled message {ScheduledMessageId}", id);
        return scheduledMessage;
    }
    
    public async Task<bool> DeleteScheduledMessage(int id)
    {
        var scheduledMessage = await uow.ScheduledMessages.GetById(id);
        if (scheduledMessage == null)
        {
            return false;
        }

        uow.ScheduledMessages.Remove(scheduledMessage);
        await uow.CompleteAsync();
        return true;
    }
    
    public async Task<bool> CanUserUpdateScheduledMessage(DiscordUserModel discordUser, int scheduledMessageId)
    {
        var scheduledMessage = await uow.ScheduledMessages.GetById(scheduledMessageId);
        return scheduledMessage != null && scheduledMessage.CreatedById == discordUser.Id;
    }
}