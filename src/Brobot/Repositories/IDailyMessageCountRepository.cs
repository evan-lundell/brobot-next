using Brobot.Models;

namespace Brobot.Repositories;

public interface IDailyMessageCountRepository : IRepository<DailyMessageCountModel, (ulong, ulong, DateOnly)>
{
    Task<IEnumerable<DailyMessageCountModel>> GetUsersTopDays(ulong userId, int numOfDays, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountModel>> GetUsersTopDaysInChannel(ulong userId, ulong channelId, int numOfDays, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountModel>> GetTopForDate(DateOnly date, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountModel>> GetTopForDateByChannel(DateOnly date, ulong channelId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountModel>> GetTotalDailyMessageCounts(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountModel>> GetTotalDailyMessageCountsByChannel(DateOnly startDate, DateOnly endDate, ulong channelId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountModel>> GetTotalTopDays(int numOfDays, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyMessageCountModel>> GetTotalTopDaysByChannel(ulong channelId, int numOfDays, CancellationToken cancellationToken = default);
}