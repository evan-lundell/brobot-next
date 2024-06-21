using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public class ScheduledMessageService : ApiServiceBase<ScheduledMessageRequest, ScheduledMessageResponse, int>, IScheduledMessageService
{
    public ScheduledMessageService(HttpClient client)
        : base("ScheduledMessages", "Reminder", client)
    {
    }
}