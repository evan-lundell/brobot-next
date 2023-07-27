using Brobot.Models;

namespace Brobot.Repositories;

public interface IDailyMessageCountRepository : IRepository<DailyMessageCountModel, (ulong, ulong, DateOnly)>
{
    Task<IEnumerable<DailyMessageCountModel>> GetUsersTopDays(ulong userId, int numOfDays);
    Task<IEnumerable<DailyMessageCountModel>> GetUsersTopDaysInChannel(ulong userId, ulong channelId, int numOfDays);
}