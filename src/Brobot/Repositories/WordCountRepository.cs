using Brobot.Contexts;
using Brobot.Models;

namespace Brobot.Repositories;

public class WordCountRepository : RepositoryBase<WordCountModel, int>, IWordCountRepository
{
    public WordCountRepository(BrobotDbContext context) 
        : base(context)
    {
    }
}
