using Brobot.Shared.Responses;
using System.Net.Http.Json;
using Blazored.Toast.Services;
using Brobot.Shared.Requests;

namespace Brobot.Frontend.Services;

public class ApiService
{
    private readonly HttpClient _client;
    private readonly ILogger _logger;

    public ApiService(
        HttpClient client, 
        ILogger<ApiService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<LoginResponse?> RefreshToken()
    {
        var response = await _client.PostAsync("auth/refresh-token", null);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse;

    }

    public async Task<LoginResponse?> Login(LoginRequest loginRequest)
    {
        var response = await _client.PostAsJsonAsync("auth/login", loginRequest);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse;
    }

    public async Task Logout()
        => await _client.PostAsync("auth/logout", null);

    public async Task<string> GetDiscordAuthUrl()
    {
        var response = await _client.GetAsync("auth/discord-auth");
        var discordAuthResponse = await response.Content.ReadFromJsonAsync<DiscordAuthResponse>();
        if (discordAuthResponse == null)
        {
            throw new Exception("Unable to get Discord Auth URL");
        }

        return discordAuthResponse.Url;
    }

    public Task SyncDiscordUser(string authCode)
        =>  _client.PostAsJsonAsync("auth/sync-discord-user", new SyncDiscordUserRequest
        {
            AuthorizationCode = authCode
        });

    public async Task<RegisterResponse?> RegisterUser(RegisterRequest registerRequest)
    {
        var response = await _client.PostAsJsonAsync("auth/register", registerRequest);
        var registerResponse = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        return registerResponse;
    }

    public async Task<ChannelResponse[]> GetChannels()
        => await _client.GetFromJsonAsync<ChannelResponse[]>("Channels") ?? Array.Empty<ChannelResponse>();

    public async Task<UserSettingsResponse> GetUserSettings()
        => await _client.GetFromJsonAsync<UserSettingsResponse>("users/settings") ?? new UserSettingsResponse();

    public async Task<UserSettingsResponse> SaveUserSettings(UserSettingsRequest userSettingsRequest)
    {
        var response = await _client.PatchAsJsonAsync("users/settings", userSettingsRequest);
        var userSettingsResponse = await response.Content.ReadFromJsonAsync<UserSettingsResponse>();
        return userSettingsResponse ?? new UserSettingsResponse();
    }

    public async Task<DailyMessageCountResponse[]> GetDailyMessageCount(int numOfDays = 10)
        => await _client.GetFromJsonAsync<DailyMessageCountResponse[]>("MessageCounts/daily") ?? Array.Empty<DailyMessageCountResponse>();

    public Task SendMessage(SendMessageRequest sendMessageRequest)
        => _client.PostAsJsonAsync("Messages/send", sendMessageRequest);

    public Task ChangePassword(ChangePasswordRequest changePasswordRequest)
        => _client.PostAsJsonAsync("auth/change-password", changePasswordRequest);

    public async Task<IdentityUserResponse[]> GetIdentityUsers()
        => await _client.GetFromJsonAsync<IdentityUserResponse[]>("Auth/users") ?? Array.Empty<IdentityUserResponse>();

    public async Task<ScheduledMessageResponse[]> GetScheduledMessages()
        => (await _client.GetFromJsonAsync<ScheduledMessageResponse[]>(
               $"ScheduledMessages?limit=50&scheduledAfter={DateTime.UtcNow:O}"))
           ?.OrderBy((sm) => sm.SendDate)
           .ToArray()
           ?? Array.Empty<ScheduledMessageResponse>();

    public async Task<ScheduledMessageResponse> EditScheduledMessage(int scheduledMessageId, ScheduledMessageRequest scheduledMessageRequest)
    {
        var response = await _client.PutAsJsonAsync($"ScheduledMessages/{scheduledMessageId}", scheduledMessageRequest);
        return await response.Content.ReadFromJsonAsync<ScheduledMessageResponse>() ?? throw new Exception("Scheduled Message failed");
    }

    public async Task DeleteScheduledMessage(int scheduledMessageId)
        => await _client.DeleteAsync($"ScheduledMessages/{scheduledMessageId}");

    public async Task<ScheduledMessageResponse> CreateScheduledMessage(ScheduledMessageRequest scheduledMessage)
    {
        var response = await _client.PostAsJsonAsync("ScheduledMessages", scheduledMessage);
        return await response.Content.ReadFromJsonAsync<ScheduledMessageResponse>() ?? throw new Exception("Something went wrong, please refresh the page");
    }
}