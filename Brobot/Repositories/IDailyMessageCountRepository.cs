using Brobot.Models;

namespace Brobot.Repositories;

public interface IDailyMessageCountRepository : IRepository<DailyMessageCountModel, ulong>
{
    Task<DailyMessageCountModel?> GetByUserAndDay(ulong userId, DateOnly date);
    Task<IEnumerable<DailyMessageCountModel>> GetByUserAndDateRange(ulong userId, DateOnly fromDate, DateOnly toDate);
}