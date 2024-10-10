using System.Web;
using Brobot.Responses;
using Newtonsoft.Json;

namespace Brobot.Services;

public class GiphyService : IGiphyService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public GiphyService(HttpClient http, IConfiguration configuration, ILogger<GiphyService> logger)
    {
        _http = http;
        _configuration = configuration;
        _logger = logger;
    }
    public async Task<string> GetGif(string? tag)
    {
        try
        {
            var queryStringBuilder = HttpUtility.ParseQueryString("");
            queryStringBuilder.Add("api_key", _configuration["GiphyApiKey"]);
            if (!string.IsNullOrWhiteSpace(tag))
            {
                queryStringBuilder.Add("tag", tag);
            }

            queryStringBuilder.Add("rating", "pg-13");

            var response = await _http.GetStringAsync($"random?{queryStringBuilder}");
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
