using System.Net.Http.Json;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public class ApiService(HttpClient client)
{
    public IHotOpService HotOpService { get; } = new HotOpService(client);
    public IScheduledMessageService ScheduledMessageService { get; } = new ScheduledMessageService(client);
    public ISecretSantaService SecretSantaService { get; } = new SecretSantaService(client);
    public IStopWordService StopWordService { get; } = new StopWordService(client);

    public async Task<LoginResponse?> RefreshToken()
    {
        var response = await client.PostAsync("auth/refresh-token", null);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse;
    }

    public async Task<LoginResponse?> DiscordLogin(string authorizationCode, string redirectUri, string state)
    {
        var request = new DiscordLoginRequest
        {
            AuthorizationCode = authorizationCode,
            RedirectUri = redirectUri,
            State = state
        };
        var response = await client.PostAsJsonAsync("auth/discord-login", request);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse;
    }

    public async Task Logout()
        => await client.PostAsync("auth/logout", null);

    public async Task<string> GetDiscordAuthUrl()
    {
        var response = await client.GetAsync("auth/discord-auth");
        var discordAuthResponse = await response.Content.ReadFromJsonAsync<DiscordAuthResponse>();
        if (discordAuthResponse == null)
        {
            throw new Exception("Unable to get Discord Auth URL");
        }

        return discordAuthResponse.Url;
    }

    public async Task<ChannelResponse[]> GetChannels()
        => await client.GetFromJsonAsync<ChannelResponse[]>("Channels") ?? [];

    public async Task<UserSettingsResponse> GetUserSettings()
        => await client.GetFromJsonAsync<UserSettingsResponse>("users/settings") ?? new UserSettingsResponse();

    public async Task<UserSettingsResponse> SaveUserSettings(UserSettingsRequest userSettingsRequest)
    {
        var response = await client.PatchAsJsonAsync("users/settings", userSettingsRequest);
        var userSettingsResponse = await response.Content.ReadFromJsonAsync<UserSettingsResponse>();
        return userSettingsResponse ?? new UserSettingsResponse();
    }

    public async Task<DailyMessageCountResponse[]> GetDailyMessageCount(ulong? channelId = null)
    {
        if (channelId == null)
        {
            return await client.GetFromJsonAsync<DailyMessageCountResponse[]>("MessageCounts/daily") ??
                   [];
        }

        return await client.GetFromJsonAsync<DailyMessageCountResponse[]>(
                   $"MessageCounts/daily?channelId={channelId.Value}") ??
               [];
    }

    public async Task<DailyMessageCountResponse[]> GetTotalDailyMessageCount(ulong? channelId = null)
    {
        if (channelId == null)
        {
            return await client.GetFromJsonAsync<DailyMessageCountResponse[]>("MessageCounts/total-daily") ??
                   [];
        }

        return await client.GetFromJsonAsync<DailyMessageCountResponse[]>(
                   $"MessageCounts/total-daily?channelId={channelId.Value}") ??
               [];
    }

    public async Task<DailyMessageCountResponse[]> GetTopDays(ulong? channelId = null)
    {
        if (channelId == null)
        {
            return await client.GetFromJsonAsync<DailyMessageCountResponse[]>("MessageCounts/top-days") ??
                   [];
        }

        return await client.GetFromJsonAsync<DailyMessageCountResponse[]>(
                   $"MessageCounts/top-days?channelId={channelId}") ??
               [];
    }

    public async Task<DailyMessageCountResponse[]> GetTodaysTopUsers(ulong? channelId = null)
    {
        if (channelId == null)
        {
            return await client.GetFromJsonAsync<DailyMessageCountResponse[]>("MessageCounts/top-today") ??
                   [];
        }

        return await client.GetFromJsonAsync<DailyMessageCountResponse[]>(
                   $"MessageCounts/top-today?channelId={channelId}") ??
               [];
    }

    public async Task<DailyMessageCountResponse[]> GetTotalTopDays(ulong? channelId = null)
    {
        if (channelId == null)
        {
            return await client.GetFromJsonAsync<DailyMessageCountResponse[]>("MessageCounts/total-top-days")
                   ?? [];
        }

        return await client.GetFromJsonAsync<DailyMessageCountResponse[]>(
                   $"MessageCounts/total-top-days?channelId={channelId.Value}") ??
               [];
    }

    public Task SendMessage(SendMessageRequest sendMessageRequest)
        => client.PostAsJsonAsync("Messages/send", sendMessageRequest);

    public async Task<ApplicationUserResponse[]> GetApplicationUsers()
        => await client.GetFromJsonAsync<ApplicationUserResponse[]>("Auth/application-users") ?? [];

    public async Task<UserResponse[]> GetUsers()
        => await client.GetFromJsonAsync<UserResponse[]>("Users/all") ?? [];
    
}