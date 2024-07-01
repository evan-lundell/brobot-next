using Brobot.Dtos;
using Brobot.Models;

namespace Brobot.Repositories;

public interface IWordCountRepository : IRepository<WordCountModel, int>
{
    Task<IEnumerable<WordCountDto>> GetWordCountsByChannelId(ulong channelId, int monthsBack = 1, int limit = 100);
}