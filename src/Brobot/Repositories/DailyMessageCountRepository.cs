using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class DailyMessageCountRepository : RepositoryBase<DailyMessageCountModel, ulong>, IDailyMessageCountRepository
{
    public DailyMessageCountRepository(BrobotDbContext context)
        : base(context)
    {
    }

    public async Task<DailyMessageCountModel?> GetByUserAndDay(ulong userId, DateOnly date)
        => await Context.DailyMessageCounts
            .SingleOrDefaultAsync((dc) => dc.UserId == userId && dc.CountDate == date);

    public async Task<IEnumerable<DailyMessageCountModel>> GetByUserAndDateRange(ulong userId, DateOnly fromDate,
        DateOnly toDate)
        => await Context.DailyMessageCounts
            .AsNoTracking()
            .Include((dc) => dc.User)
            .Where((dc) => dc.UserId == userId && dc.CountDate >= fromDate && dc.CountDate <= toDate)
            .OrderBy((dc) => dc.CountDate)
            .ToListAsync();
}