using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class StatPeriodRepository(BrobotDbContext context)
    : RepositoryBase<StatPeriodModel, int>(context), IStatPeriodRepository
{
    public Task<StatPeriodModel?> GetStatPeriodWithCounts(int statPeriodId) =>
        Context.StatPeriods
            .AsSplitQuery()
            .Include(sp => sp.WordCounts)
            .Include(sp => sp.UserMessageCounts)
            .SingleOrDefaultAsync(sp => sp.Id == statPeriodId);
}