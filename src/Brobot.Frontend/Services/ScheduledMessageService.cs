using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.WebUtilities;

namespace Brobot.Frontend.Services;

public class ScheduledMessageService : ApiServiceBase<ScheduledMessageRequest, ScheduledMessageResponse, int>, IScheduledMessageService
{
    public ScheduledMessageService(HttpClient client)
        : base("ScheduledMessages", "Reminder", client)
    {
    }

    public async Task<IEnumerable<ScheduledMessageResponse>> GetUnsentScheduledMessages(int page = 1)
    {
        Dictionary<string, string?> queryParams = new()
        {
            { "limit", "10" },
            { "skip", (page * 10).ToString() },
            { "scheduledAfter", DateTime.UtcNow.ToString("o") }
        };
        return await GetAll(QueryHelpers.AddQueryString(BaseUrl, queryParams));
    }

    public Task<IEnumerable<ScheduledMessageResponse>> GetSentScheduledMessages(int page = 1)
    {
        Dictionary<string, string?> queryParams = new()
        {
            { "limit", "10" },
            { "skip", (page * 10).ToString() },
            { "scheduledBefore", DateTime.UtcNow.ToString("o") }
        };
        return GetAll(QueryHelpers.AddQueryString(BaseUrl, queryParams));
    }
}