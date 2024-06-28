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
        time ??= DateTime.UtcNow;
        var messages = await Context.ScheduledMessages
            .Where(m => time >= m.SendDate && time < m.SendDate.Value.AddMinutes(1) && m.SentDate == null)
            .ToListAsync();

        return messages;
    }

    public async Task<IEnumerable<ScheduledMessageModel>> GetScheduledMessagesByUser(ulong userId, int? limit = null,
        int skip = 0, DateTime? scheduledBefore = null, DateTime? scheduledAfter = null)
    {
        var query = Context.ScheduledMessages
            .Include(sm => sm.Channel)
            .Include(sm => sm.CreatedBy)
            .Where(sm => sm.CreatedById == userId);

        if (scheduledBefore.HasValue)
        {
            query = query.Where(sm => sm.SendDate.HasValue && sm.SendDate.Value < scheduledBefore);
        }

        if (scheduledAfter.HasValue)
        {
            query = query.Where(sm => sm.SendDate.HasValue && sm.SendDate >= scheduledAfter);
        }
        
        if (skip > 0)
        {
            query = query.Skip(skip);
        }

        query = query.OrderBy(sm => sm.SendDate);
        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query
            .ToListAsync();
    }
}