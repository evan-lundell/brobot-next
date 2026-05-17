using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class StopWordRepository(BrobotDbContext context)
    : RepositoryBase<StopWordModel, int>(context), IStopWordRepository
{
    public async Task<bool> StopWordExists(string word, CancellationToken cancellationToken = default)
    {
        var existingWord =
            await Context.StopWords.SingleOrDefaultAsync(sw => word == sw.Word, cancellationToken);

        return existingWord != null;
    }

    public Task<StopWordModel?> GetByWord(string word, CancellationToken cancellationToken = default)
        => Context.StopWords.SingleOrDefaultAsync(sw => word == sw.Word, cancellationToken);
}