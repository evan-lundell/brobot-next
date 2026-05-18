using Brobot.Models;

namespace Brobot.Repositories;

public interface IStopWordRepository : IRepository<StopWordModel, int>
{
    Task<StopWordModel?> GetByWord(string word, CancellationToken cancellationToken = default);
    Task<bool> StopWordExists(string word, CancellationToken cancellationToken = default);
}