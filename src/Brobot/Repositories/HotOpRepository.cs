using System.Linq.Expressions;
using Brobot.Contexts;
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
        var activeHotOps = await Context.HotOps
            .AsNoTracking()
            .Include(ho => ho.User)
            .Include(ho => ho.HotOpSessions)
            .ThenInclude(hos => hos.User)
            .Include(ho => ho.Channel)
            .ThenInclude(c => c.ChannelUsers)
            .ThenInclude(cu => cu.User)
            .Where(ho => ho.ChannelId == channelId && ho.StartDate <= utcNow && ho.EndDate > utcNow)
            .ToListAsync();

        return activeHotOps;
    }

    public async Task<IEnumerable<HotOpModel>> GetUsersHotOps(ulong userId, HotOpQueryType type = HotOpQueryType.All)
    {
        var utcNow = DateTime.UtcNow;
        IQueryable<HotOpModel> query = Context.HotOps
            .Include(ho => ho.Channel)
            .Include(ho => ho.User)
            .Where(ho => ho.Channel.ChannelUsers.Any(cu => cu.UserId == userId));

        switch (type)
        {
            case HotOpQueryType.Completed:
                query = query.Where(ho => ho.EndDate < utcNow);
                break;
            case HotOpQueryType.Current:
                query = query.Where(ho => ho.StartDate < utcNow && ho.EndDate > utcNow);
                break;
            case HotOpQueryType.Upcoming:
                query = query.Where(ho => ho.StartDate > utcNow);
                break;
            case HotOpQueryType.All:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        
        return await query.ToListAsync();
    }

    public override async Task<IEnumerable<HotOpModel>> Find(Expression<Func<HotOpModel, bool>> expression)
    {
        return await Context.HotOps
            .Include(ho => ho.User)
            .Include(ho => ho.HotOpSessions)
            .ThenInclude(hos => hos.User)
            .Include(ho => ho.Channel)
            .ThenInclude(c => c.ChannelUsers)
            .ThenInclude(cu => cu.User)
            .Where(expression)
            .ToListAsync();
    }

    public override Task<HotOpModel?> GetById(int id)
        => Context.HotOps
            .Include(ho => ho.User)
            .Include(ho => ho.HotOpSessions)
            .ThenInclude(hos => hos.User)
            .Include(ho => ho.Channel)
            .ThenInclude(c => c.ChannelUsers)
            .ThenInclude(cu => cu.User)
            .SingleOrDefaultAsync(ho => ho.Id == id);
}