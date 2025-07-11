using Brobot.Contexts;
using Brobot.Dtos;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class WordCountRepository(BrobotDbContext context)
    : RepositoryBase<WordCountModel, int>(context), IWordCountRepository
{
    public async Task<IEnumerable<WordCountDto>> GetWordCountsByChannelId(ulong channelId, DateOnly startDate, DateOnly endDate, int limit = 100)
    {
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
