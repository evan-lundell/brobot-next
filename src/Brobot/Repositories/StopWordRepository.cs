using Brobot.Contexts;
using Brobot.Models;
using Brobot.Services;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class StopWordRepository : RepositoryBase<StopWordModel, int>, IStopWordRepository
{

    public StopWordRepository(BrobotDbContext context) 
        : base(context)
    {
    }

    public async Task<bool> StopWordExists(string word)
    {
        var existingWord =
            await Context.StopWords.SingleOrDefaultAsync((sw) => word == sw.Word);

        return existingWord != null;
    }

    public Task<StopWordModel?> GetByWord(string word)
        => Context.StopWords.SingleOrDefaultAsync((sw) => word == sw.Word);
}