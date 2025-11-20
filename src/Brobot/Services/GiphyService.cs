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
        var queryStringBuilder = HttpUtility.ParseQueryString("");
        queryStringBuilder.Add("api_key", options.Value.GiphyApiKey);
        if (!string.IsNullOrWhiteSpace(tag))
        {
            logger.LogInformation("Getting Giphy gif with tag {Tag}", tag);
            queryStringBuilder.Add("tag", tag);
        }
        else
        {
            logger.LogInformation("Getting Giphy gif");
        }

        queryStringBuilder.Add("rating", "pg-13");

        var response = await http.GetStringAsync($"random?{queryStringBuilder}");
        var giphy = JsonConvert.DeserializeObject<GiphyResponse>(response);
        var url = giphy?.Data?.Url ?? "";

        if (string.IsNullOrWhiteSpace(url))
        {
            logger.LogWarning("Unable to parse giphy response");
        }
        else
        {
            logger.LogInformation("Giphy response {Url}", url);
        }

        return url;
    }
}
