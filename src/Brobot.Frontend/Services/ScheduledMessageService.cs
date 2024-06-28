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
            { "scheduledAfter", DateTime.UtcNow.ToString("o") }
        };
        var url = QueryHelpers.AddQueryString(BaseUrl, queryParams);
        return await GetAll(url);
    }

    public Task<IEnumerable<ScheduledMessageResponse>> GetSentScheduledMessages(int page = 1)
    {
        Dictionary<string, string?> queryParams = new()
        {
            { "scheduledBefore", DateTime.UtcNow.ToString("o") }
        };
        return GetAll(QueryHelpers.AddQueryString(BaseUrl, queryParams));
    }
}