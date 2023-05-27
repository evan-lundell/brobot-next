using Brobot.Responses;
using Newtonsoft.Json;

namespace Brobot.Services;

public class DictionaryService : IDictionaryService
{
    private readonly HttpClient _http;

    public DictionaryService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GetDefinition(string word)
    {
        var response = await _http.GetAsync($"api/v2/entries/en/{word}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return "That's not a word dummy";
        }
        var responseJson = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<DictionaryResponse[]>(responseJson);
        if (data == null || data.Length == 0)
        {
            return "An error occured";
        }
        var meaningData = data[0];
        if (meaningData.Meanings == null || meaningData.Meanings.Length == 0)
        {
            return "An error occured";
        }

        var meanings = new List<string>();
        var count = 1;
        foreach (var meaning in meaningData.Meanings)
        {
            if (meaning.Definitions == null)
            {
                continue;
            }
            meanings.Add($"{count}: ({meaning.PartOfSpeech}) {meaning.Definitions[0].Definition}");
            count++;
        }

        return string.Join("\n", meanings);
    }
}