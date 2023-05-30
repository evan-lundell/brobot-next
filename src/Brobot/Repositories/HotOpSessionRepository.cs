using Brobot.Contexts;
using Brobot.Models;

namespace Brobot.Repositories;

public class HotOpSessionRepository : RepositoryBase<HotOpSessionModel, int>, IHotOpSessionRepository
{
    public HotOpSessionRepository(BrobotDbContext context)
        : base(context)
    {
    }
}