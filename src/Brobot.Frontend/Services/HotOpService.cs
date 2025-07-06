using System.Net.Http.Json;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public class HotOpService(HttpClient httpClient)
    : ApiServiceBase<HotOpRequest, HotOpResponse, int>("api/HotOps", "Hot Op", httpClient), IHotOpService
{
    public Task<IEnumerable<HotOpResponse>> GetUpcomingHotOps()
        => GetAll($"{BaseUrl}?type=Upcoming");

    public Task<IEnumerable<HotOpResponse>> GetCurrentHotOps()
        => GetAll($"{BaseUrl}?type=current");

    public Task<IEnumerable<HotOpResponse>> GetCompletedHotOps()
        => GetAll($"{BaseUrl}?type=completed");

    public async Task<ScoreboardResponse> GetHotOpScoreboard(int hotOpId)
    {
        return await HttpClient.GetFromJsonAsync<ScoreboardResponse>($"{BaseUrl}/{hotOpId}/scoreboard")
            ?? new ScoreboardResponse { OwnerUsername = "", Scores = Array.Empty<ScoreboardItemResponse>() };
    }
}