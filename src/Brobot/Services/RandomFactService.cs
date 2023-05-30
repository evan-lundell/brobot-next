using Brobot.Responses;
using Newtonsoft.Json;

namespace Brobot.Services;

public class RandomFactService : IRandomFactService
{
    private readonly HttpClient _http;

    public RandomFactService(HttpClient http)
    {
        _http = http;
    }
    public async Task<string> GetFact()
    {
        var response = await _http.GetStringAsync("random.json?language=en");
        var randomFact = JsonConvert.DeserializeObject<RandomFactResponse>(response);
        return randomFact?.Text ?? "";
    }
}