using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public class StopWordService(HttpClient httpClient)
    : ApiServiceBase<StopWordRequest, StopWordResponse, int>("api/StopWords", "StopWord", httpClient), IStopWordService
{
    public Task DeleteByWord(string word)
        => Delete($"{BaseUrl}/{word}");
}