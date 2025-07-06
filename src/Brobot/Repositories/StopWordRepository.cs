using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class StopWordRepository(BrobotDbContext context)
    : RepositoryBase<StopWordModel, int>(context), IStopWordRepository
{
    public async Task<bool> StopWordExists(string word)
    {
        var existingWord =
            await Context.StopWords.SingleOrDefaultAsync(sw => word == sw.Word);

        return existingWord != null;
    }

    public Task<StopWordModel?> GetByWord(string word)
        => Context.StopWords.SingleOrDefaultAsync(sw => word == sw.Word);
}