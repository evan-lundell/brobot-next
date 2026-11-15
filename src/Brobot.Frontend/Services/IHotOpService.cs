using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public interface IHotOpService : IApiService<HotOpRequest, HotOpResponse, int>
{
    Task<IEnumerable<HotOpResponse>> GetUpcomingHotOps(bool allUsers = false);
    Task<IEnumerable<HotOpResponse>> GetCurrentHotOps();
    Task<IEnumerable<HotOpResponse>> GetCompletedHotOps();
    Task<ScoreboardResponse> GetHotOpScoreboard(int hotOpId);
}