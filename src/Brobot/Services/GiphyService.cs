using System.Web;
using Brobot.Responses;
using Newtonsoft.Json;

namespace Brobot.Services;

public class GiphyService(HttpClient http, IConfiguration configuration, ILogger<GiphyService> logger)
    : IGiphyService
{
    private readonly ILogger _logger = logger;

    public async Task<string> GetGif(string? tag)
    {
        try
        {
            var queryStringBuilder = HttpUtility.ParseQueryString("");
            queryStringBuilder.Add("api_key", configuration["GiphyApiKey"]);
            if (!string.IsNullOrWhiteSpace(tag))
            {
                queryStringBuilder.Add("tag", tag);
            }

            queryStringBuilder.Add("rating", "pg-13");

            var response = await http.GetStringAsync($"random?{queryStringBuilder}");
            var giphy = JsonConvert.DeserializeObject<GiphyResponse>(response);
            return giphy?.Data?.Url ?? "";
        }
        catch (HttpRequestException hre)
        {
            _logger.LogError(hre, "Error getting Giphy gif");
            return "";
        }
    }
}
