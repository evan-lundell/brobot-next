using Brobot.Models;

namespace Brobot.Repositories;

public interface IScheduledMessageRepository : IRepository<ScheduledMessageModel, int>
{
    Task<IEnumerable<ScheduledMessageModel>> GetActiveMessages(DateTime? time = null);
}