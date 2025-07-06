using Brobot.Responses;
using Newtonsoft.Json;

namespace Brobot.Services;

public class RandomFactService(HttpClient http) : IRandomFactService
{
    public async Task<string> GetFact()
    {
        var response = await http.GetStringAsync("random.json?language=en");
        var randomFact = JsonConvert.DeserializeObject<RandomFactResponse>(response);
        return randomFact?.Text ?? "";
    }
}
