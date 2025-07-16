using System.Web;
using Brobot.Configuration;
using Brobot.Responses;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Brobot.Services;

public class GiphyService(HttpClient http, IOptions<ExternalApisOptions> options, ILogger<GiphyService> logger)
    : IGiphyService
{
    public async Task<string> GetGif(string? tag)
    {
        try
        {
            var queryStringBuilder = HttpUtility.ParseQueryString("");
            queryStringBuilder.Add("api_key", options.Value.GiphyApiKey);
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
            logger.LogError(hre, "Error getting Giphy gif");
            return "";
        }
    }
}
