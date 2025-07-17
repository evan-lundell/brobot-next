using Brobot.Models;

namespace Brobot.Repositories;

public interface IVersionRepository : IRepository<VersionModel, int>
{
    Task<VersionModel?> GetLatestVersion();
}