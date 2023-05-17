using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class ScheduledMessageRepository : RepositoryBase<ScheduledMessageModel, int>, IScheduledMessageRepository
{
    public ScheduledMessageRepository(BrobotDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<ScheduledMessageModel>> GetActiveMessages(DateTime? time = null)
    {
        if (time == null)
        {
            time = DateTime.UtcNow;
        }
        var messages = await _context.ScheduledMessages
            .Where((m) => time >= m.SendDate && time < m.SendDate.Value.AddMinutes(1) && m.SentDate == null)
            .ToListAsync();

        return messages;
    }

    public async Task<IEnumerable<ScheduledMessageModel>> GetScheduledMessagesByUser(ulong userId, int limit = 10, int skip = 0, DateTime? scheduledBefore = null, DateTime? scheduledAfter = null)
    {
        IQueryable<ScheduledMessageModel> query = _context.ScheduledMessages
            .Take(limit)
            .Skip(skip);

        if (scheduledBefore.HasValue)
        {
            query = query.Where((sm) => sm.SendDate.HasValue && sm.SendDate.Value < scheduledBefore);
        }

        if (scheduledAfter.HasValue)
        {
            query = query.Where((sm) => sm.SendDate.HasValue && sm.SendDate >= scheduledAfter);
        }

        return await query.ToListAsync();
    }
}