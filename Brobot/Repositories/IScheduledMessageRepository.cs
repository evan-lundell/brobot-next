using Brobot.Models;

namespace Brobot.Repositories;

public interface IScheduledMessageRepository : IRepository<ScheduledMessageModel, int>
{
    Task<IEnumerable<ScheduledMessageModel>> GetActiveMessages(DateTime? time = null);
    Task<IEnumerable<ScheduledMessageModel>> GetScheduledMessagesByUser(ulong userId, int limit = 10, int skip = 0, DateTime? scheduledBefore = null, DateTime? scheduledAfter = null);
}