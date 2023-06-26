using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public interface IScheduledMessageService : IApiService<ScheduledMessageRequest, ScheduledMessageResponse, int>
{
}