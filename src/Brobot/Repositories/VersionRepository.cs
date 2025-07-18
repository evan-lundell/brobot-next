using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class VersionRepository(BrobotDbContext context) : RepositoryBase<VersionModel, int>(context), IVersionRepository
{
    public async Task<VersionModel?> GetLatestVersion()
    {
        var latestVersion = await Context.Versions
            .OrderByDescending(v => v.VersionDate)
            .FirstOrDefaultAsync();
        return latestVersion;
    }
}