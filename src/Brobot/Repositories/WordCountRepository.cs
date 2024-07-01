using Brobot.Contexts;
using Brobot.Dtos;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Brobot.Repositories;

public class WordCountRepository : RepositoryBase<WordCountModel, int>, IWordCountRepository
{
    public WordCountRepository(BrobotDbContext context) 
        : base(context)
    {
    }

    public async Task<IEnumerable<WordCountDto>> GetWordCountsByChannelId(ulong channelId, int monthsBack = 1, int limit = 100)
    {
        var startDateTime = DateTime.UtcNow.AddDays(-monthsBack);
        var startDate = new DateOnly(startDateTime.Year, startDateTime.Month, 1);
        var endDate = startDate.AddMonths(1);
        
        var query = Context.WordCounts
            .GroupJoin(Context.StopWords, wc => wc.Word, sw => sw.Word, (wc, sw) => new { wc, sw })
            .SelectMany(temp => temp.sw.DefaultIfEmpty(), (temp, sw) => new { temp.wc, sw })
            .Where(temp => temp.wc.CountDate >= startDate && temp.wc.CountDate < endDate && temp.wc.ChannelId == channelId && temp.sw == null)
            .GroupBy(temp => temp.wc.Word)
            .Select(g => new WordCountDto { Word = g.Key, Count = g.Sum(temp => temp.wc.Count) })
            .OrderByDescending(temp => temp.Count)
            .Take(limit);
        return await query.ToListAsync();
    }
}
