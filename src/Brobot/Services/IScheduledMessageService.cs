using Brobot.Models;

namespace Brobot.Services;

public interface IScheduledMessageService
{
    Task<IEnumerable<ScheduledMessageModel>> GetScheduledMessagesByUser(DiscordUserModel discordUser, int? limit = null, int skip = 0,
        DateTime? scheduledBefore = null, DateTime? scheduledAfter = null);

    Task<ScheduledMessageModel> CreateScheduledMessage(string messageText, DiscordUserModel createdBy,
        DateTime sendDate, ulong channelId);

    Task<ScheduledMessageModel> UpdateScheduledMessage(int id, string? text = null, ulong? channelId = null, DateTime? sendDate = null);
    Task<bool> DeleteScheduledMessage(int id);
    Task<bool> CanUserUpdateScheduledMessage(DiscordUserModel discordUser, int scheduledMessageId);
}