using Brobot.Dtos;
using Brobot.Models;

namespace Brobot.Services;

public interface IWordCountService
{
    Task<IEnumerable<WordCountDto>> GetWordCount(ChannelModel channel, DateTime startDate, DateTime endDate);
}