using Brobot.Models;
using Brobot.Shared;

namespace Brobot.Repositories;

public interface IHotOpRepository : IRepository<HotOpModel, int>
{
    Task<IEnumerable<HotOpModel>> GetActiveHotOpsWithSessions(ulong channelId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HotOpModel>> GetUsersHotOps(ulong userId, HotOpQueryType type = HotOpQueryType.All, CancellationToken cancellationToken = default);
}