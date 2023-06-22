using System.Text.Json;
using System.Web;
using Brobot.Shared.Responses;

namespace Brobot.Services;

public class SongDataService
{
    private readonly HttpClient _client;

    public SongDataService(HttpClient client)
    {
        _client = client;
    }

    public async Task<SongDataResponse> GetSongData(string url)
    {
        var response = await _client.GetAsync($"https://youtube.com/oembed?url={HttpUtility.UrlEncode(url)}&format=json");
        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        var songData = new SongDataResponse();
        if (jsonResponse.TryGetProperty("author_name", out var nameElement))
        {
            songData.Artist = nameElement.GetString();
        }

        if (jsonResponse.TryGetProperty("title", out var titleElement))
        {
            songData.Name = titleElement.GetString();
        }

        return songData;
    }
}