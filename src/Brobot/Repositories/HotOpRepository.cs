using System.Linq.Expressions;
using Brobot.Contexts;
using Brobot.Dtos;
using Brobot.Models;
using Brobot.Shared;
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

    public async Task<IEnumerable<HotOpModel>> GetUsersHotOps(ulong userId, HotOpQueryType type = HotOpQueryType.All)
    {
        var whereClause = "WHERE cu.user_id = {0}";
        var utcNow = DateTime.UtcNow;
        switch (type)
        {
            case HotOpQueryType.Completed:
                whereClause += " AND end_date < {1}";
                break;
            case HotOpQueryType.Current:
                whereClause += " AND start_date < {1} AND end_date > {1}";
                break;
            case HotOpQueryType.Upcoming:
                whereClause += " AND start_date > {1}";
                break;
        }

        var hotOps = await Context.HotOps.FromSqlRaw(
            $"SELECT ho.* FROM brobot.hot_op ho INNER JOIN brobot.channel_user cu ON ho.channel_id = cu.channel_id {whereClause}",
            userId, 
            utcNow)
            .Include((ho) => ho.Channel)
            .Include((ho) => ho.User)
            .ToListAsync();
        return hotOps;
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

    public override Task<HotOpModel?> GetById(int id)
        => Context.HotOps
            .Include((ho) => ho.User)
            .Include((ho) => ho.HotOpSessions)
            .ThenInclude((hos) => hos.User)
            .Include((ho) => ho.Channel)
            .ThenInclude((c) => c.ChannelUsers)
            .ThenInclude((cu) => cu.User)
            .SingleOrDefaultAsync((ho) => ho.Id == id);
}