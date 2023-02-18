using Brobot.Contexts;
using Brobot.Models;

namespace Brobot.Repositories;

public class ScheduledMessageRepository : RepositoryBase<ScheduledMessageModel, int>, IScheduledMessageRepository
{
    public ScheduledMessageRepository(BrobotDbContext context)
        : base(context)
    {
    }
}