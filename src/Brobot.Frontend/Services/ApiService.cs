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

    public async Task<LoginResponse?> Login(LoginRequest loginRequest)
    {
        var response = await client.PostAsJsonAsync("auth/login", loginRequest);
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

    public Task SyncDiscordUser(string authCode)
        => client.PostAsJsonAsync("auth/sync-discord-user", new SyncDiscordUserRequest
        {
            AuthorizationCode = authCode
        });

    public async Task<RegisterResponse?> RegisterUser(RegisterRequest registerRequest)
    {
        var response = await client.PostAsJsonAsync("auth/register", registerRequest);
        var registerResponse = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        return registerResponse;
    }

    public async Task<ChannelResponse[]> GetChannels()
        => await client.GetFromJsonAsync<ChannelResponse[]>("Channels") ?? Array.Empty<ChannelResponse>();

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
                   Array.Empty<DailyMessageCountResponse>();
        }

        return await client.GetFromJsonAsync<DailyMessageCountResponse[]>(
                   $"MessageCounts/daily?channelId={channelId.Value}") ??
               Array.Empty<DailyMessageCountResponse>();
    }

    public async Task<DailyMessageCountResponse[]> GetTotalDailyMessageCount(ulong? channelId = null)
    {
        if (channelId == null)
        {
            return await client.GetFromJsonAsync<DailyMessageCountResponse[]>("MessageCounts/total-daily") ??
                   Array.Empty<DailyMessageCountResponse>();
        }

        return await client.GetFromJsonAsync<DailyMessageCountResponse[]>(
                   $"MessageCounts/total-daily?channelId={channelId.Value}") ??
               Array.Empty<DailyMessageCountResponse>();
    }

    public async Task<DailyMessageCountResponse[]> GetTopDays(ulong? channelId = null)
    {
        if (channelId == null)
        {
            return await client.GetFromJsonAsync<DailyMessageCountResponse[]>("MessageCounts/top-days") ??
                   Array.Empty<DailyMessageCountResponse>();
        }

        return await client.GetFromJsonAsync<DailyMessageCountResponse[]>(
                   $"MessageCounts/top-days?channelId={channelId}") ??
               Array.Empty<DailyMessageCountResponse>();
    }

    public async Task<DailyMessageCountResponse[]> GetTodaysTopUsers(ulong? channelId = null)
    {
        if (channelId == null)
        {
            return await client.GetFromJsonAsync<DailyMessageCountResponse[]>("MessageCounts/top-today") ??
                   Array.Empty<DailyMessageCountResponse>();
        }

        return await client.GetFromJsonAsync<DailyMessageCountResponse[]>(
                   $"MessageCounts/top-today?channelId={channelId}") ??
               Array.Empty<DailyMessageCountResponse>();
    }

    public async Task<DailyMessageCountResponse[]> GetTotalTopDays(ulong? channelId = null)
    {
        if (channelId == null)
        {
            return await client.GetFromJsonAsync<DailyMessageCountResponse[]>("MessageCounts/total-top-days")
                   ?? Array.Empty<DailyMessageCountResponse>();
        }

        return await client.GetFromJsonAsync<DailyMessageCountResponse[]>(
                   $"MessageCounts/total-top-days?channelId={channelId.Value}") ??
               Array.Empty<DailyMessageCountResponse>();
    }

    public Task SendMessage(SendMessageRequest sendMessageRequest)
        => client.PostAsJsonAsync("Messages/send", sendMessageRequest);

    public Task ChangePassword(ChangePasswordRequest changePasswordRequest)
        => client.PostAsJsonAsync("auth/change-password", changePasswordRequest);

    public async Task<IdentityUserResponse[]> GetIdentityUsers()
        => await client.GetFromJsonAsync<IdentityUserResponse[]>("Auth/users") ?? Array.Empty<IdentityUserResponse>();

    public async Task<UserResponse[]> GetUsers()
        => await client.GetFromJsonAsync<UserResponse[]>("Users/all") ?? Array.Empty<UserResponse>();
    
}