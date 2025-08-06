using Brobot.Models;

namespace Brobot.Repositories;

public interface IStatPeriodRepository : IRepository<StatPeriodModel, int>
{
    Task<StatPeriodModel?> GetStatPeriodWithCounts(int statPeriodId);
}