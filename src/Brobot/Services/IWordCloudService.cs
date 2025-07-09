using Brobot.Dtos;

namespace Brobot.Services;

public interface IWordCloudService
{
    Task<byte[]> GetWordCloud(IEnumerable<WordCountDto> wordCounts);
}