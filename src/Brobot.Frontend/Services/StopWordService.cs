using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public class StopWordService : ApiServiceBase<StopWordRequest, StopWordResponse, int>, IStopWordService
{
    public StopWordService(HttpClient httpClient) 
        : base("api/StopWords", "StopWord", httpClient)
    {
    }

    public Task DeleteByWord(string word)
        => Delete($"{BaseUrl}/{word}");
}