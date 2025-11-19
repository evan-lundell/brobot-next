using Brobot.Responses;
using Newtonsoft.Json;

namespace Brobot.Services;

public class RandomFactService(HttpClient http, ILogger<RandomFactService> logger) : IRandomFactService
{
    public async Task<string> GetFact()
    {
        logger.LogInformation("Getting random fact");
        var response = await http.GetStringAsync("random.json?language=en");
        var randomFact = JsonConvert.DeserializeObject<RandomFactResponse>(response);
        logger.LogInformation("Finished getting random fact");
        return randomFact?.Text ?? "";
    }
}
