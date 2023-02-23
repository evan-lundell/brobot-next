using Brobot.Dtos;
using Brobot.Models;

namespace Brobot.Repositories;

public interface IHotOpRepository : IRepository<HotOpModel, int>
{
    Task<IEnumerable<ScoreboardDto>> GetActiveHotOpScoreboards();
}