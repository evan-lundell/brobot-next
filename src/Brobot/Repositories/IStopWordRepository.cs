using Brobot.Models;

namespace Brobot.Repositories;

public interface IStopWordRepository : IRepository<StopWordModel, int>
{
    Task<StopWordModel?> GetByWord(string word);
    Task<bool> StopWordExists(string word);
}