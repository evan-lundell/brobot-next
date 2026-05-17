using Brobot.Models;

namespace Brobot.Services;

public interface IScheduledMessageService
{
    Task<IEnumerable<ScheduledMessageModel>> GetScheduledMessagesByUser(DiscordUserModel discordUser, int? limit = null, int skip = 0,
        DateTime? scheduledBefore = null, DateTime? scheduledAfter = null, CancellationToken cancellationToken = default);

    Task<ScheduledMessageModel> CreateScheduledMessage(string messageText, DiscordUserModel createdBy,
        DateTime sendDate, ulong channelId, CancellationToken cancellationToken = default);

    Task<ScheduledMessageModel> UpdateScheduledMessage(int id, string? text = null, ulong? channelId = null, DateTime? sendDate = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteScheduledMessage(int id, CancellationToken cancellationToken = default);
    Task<bool> CanUserUpdateScheduledMessage(DiscordUserModel discordUser, int scheduledMessageId, CancellationToken cancellationToken = default);
}