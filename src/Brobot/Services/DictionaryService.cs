using System.Net;
using Brobot.Responses;
using Newtonsoft.Json;

namespace Brobot.Services;

public class DictionaryService(HttpClient http) : IDictionaryService
{
    public async Task<string> GetDefinition(string word)
    {
        var response = await http.GetAsync($"api/v2/entries/en/{word}");
        if (response.StatusCode == HttpStatusCode.NotFound)
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