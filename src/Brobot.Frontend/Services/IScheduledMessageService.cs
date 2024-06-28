using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public interface IScheduledMessageService : IApiService<ScheduledMessageRequest, ScheduledMessageResponse, int>
{
    public Task<IEnumerable<ScheduledMessageResponse>> GetUnsentScheduledMessages(int page = 1);
    public Task<IEnumerable<ScheduledMessageResponse>> GetSentScheduledMessages(int page = 1);
}