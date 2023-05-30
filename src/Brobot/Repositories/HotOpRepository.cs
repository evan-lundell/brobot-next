using System.Linq.Expressions;
using Brobot.Contexts;
using Brobot.Dtos;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class HotOpRepository : RepositoryBase<HotOpModel, int>, IHotOpRepository
{
    public HotOpRepository(BrobotDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<HotOpModel>> GetActiveHotOpsWithSessions(ulong channelId)
    {
        var utcNow = DateTime.UtcNow;
        var scoreboards = new List<ScoreboardDto>();
        var activeHotOps = await Context.HotOps
            .AsNoTracking()
            .Include((ho) => ho.User)
            .Include((ho) => ho.HotOpSessions)
            .ThenInclude((hos) => hos.User)
            .Include((ho) => ho.Channel)
            .ThenInclude((c) => c.ChannelUsers)
            .ThenInclude((cu) => cu.User)
            .Where((ho) => ho.ChannelId == channelId && ho.StartDate <= utcNow && ho.EndDate > utcNow)
            .ToListAsync();

        return activeHotOps;
    }

    public override async Task<IEnumerable<HotOpModel>> Find(Expression<Func<HotOpModel, bool>> expression)
    {
        return await Context.HotOps
            .Include((ho) => ho.User)
            .Include((ho) => ho.HotOpSessions)
            .ThenInclude((hos) => hos.User)
            .Include((ho) => ho.Channel)
            .ThenInclude((c) => c.ChannelUsers)
            .ThenInclude((cu) => cu.User)
            .Where(expression)
            .ToListAsync();
    }
}