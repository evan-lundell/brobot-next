using Brobot.Dtos;

namespace Brobot.Services;


public class WordCloudService(HttpClient http, ILogger<WordCloudService> logger) : IWordCloudService
{
    public async Task<byte[]> GetWordCloud(IEnumerable<WordCountDto> wordCounts)
    {
        try
        {
            var text = string.Join(",", wordCounts.Select(wc => $"{wc.Word}:{wc.Count}"));
            var response = await http.PostAsJsonAsync("wordcloud", new
            {
                text,
                useWordList = true,
                format = "png",
                fontSize = 8,
                height = 1000,
                width = 1000
            });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting wordcloud");
            return [];
        }
    }
}
