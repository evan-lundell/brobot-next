using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public interface IStopWordService : IApiService<StopWordRequest, StopWordResponse, int>
{
    Task DeleteByWord(string word);
}