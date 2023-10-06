using Brobot.Models;

namespace Brobot.Repositories;

public interface IDailyMessageCountRepository : IRepository<DailyMessageCountModel, (ulong, ulong, DateOnly)>
{
    Task<IEnumerable<DailyMessageCountModel>> GetUsersTopDays(ulong userId, int numOfDays);
    Task<IEnumerable<DailyMessageCountModel>> GetUsersTopDaysInChannel(ulong userId, ulong channelId, int numOfDays);
    Task<IEnumerable<DailyMessageCountModel>> GetTopForDate(DateOnly date);
    Task<IEnumerable<DailyMessageCountModel>> GetTopForDateByChannel(DateOnly date, ulong channelId);
    Task<IEnumerable<DailyMessageCountModel>> GetTotalDailyMessageCounts(DateOnly startDate, DateOnly endDate);
    Task<IEnumerable<DailyMessageCountModel>> GetTotalDailyMessageCountsByChannel(DateOnly startDate, DateOnly endDate, ulong channelId);
    Task<IEnumerable<DailyMessageCountModel>> GetTotalTopDays(int numOfDays);
    Task<IEnumerable<DailyMessageCountModel>> GetTotalTopDaysByChannel(ulong channelId, int numOfDays);
}