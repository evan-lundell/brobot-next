using Brobot.Dtos;
using Brobot.Models;

namespace Brobot.Repositories;

public interface IWordCountRepository : IRepository<WordCountModel, int>
{
    Task<IEnumerable<WordCountDto>> GetWordCountsByChannelId(ulong channelId, DateOnly startDate, DateOnly endDate, int limit = 100);
}