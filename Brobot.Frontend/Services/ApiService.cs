using Brobot.Shared.Responses;
using System.Net.Http.Json;
using Brobot.Shared;
using Brobot.Shared.Requests;

namespace Brobot.Frontend.Services;

public class ApiService
{
    private readonly HttpClient _client;

    public ApiService(HttpClient client)
    {
        _client = client;
    }

    public async Task<LoginResponse?> RefreshToken()
    {

        var response = await _client.PostAsync("auth/refresh-token", null);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse;

    }

    public async Task<WeatherForecast[]> GetWeatherForecast()
    {
        var response = await _client.GetAsync("WeatherForecast");
        var weatherForecast = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        if (weatherForecast == null)
        {
            return new WeatherForecast[0];
        }
        return weatherForecast;
    }

    public async Task<LoginResponse?> Login(LoginRequest loginRequest)
    {
        var response = await _client.PostAsJsonAsync("auth/login", loginRequest);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse;
    }

    public async Task Logout()
    {
        var response = await _client.PostAsync("auth/logout", null);
    }

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

    public async Task SyncDiscordUser(string authCode)
    {
        var response = await _client.PostAsJsonAsync<SyncDiscordUserRequest>("auth/sync-discord-user", new SyncDiscordUserRequest
        {
            AuthorizationCode = authCode
        });
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<RegisterResponse?> RegisterUser(RegisterRequest registerRequest)
    {
        var response = await _client.PostAsJsonAsync<RegisterRequest>("auth/register", registerRequest);
        var registerResponse = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        return registerResponse;
    }

    public async Task<ChannelResponse[]> GetChannels()
    {
        try
        {
            var response = await _client.GetAsync("channels");
            var channels = await response.Content.ReadFromJsonAsync<ChannelResponse[]>();
            return channels ?? Array.Empty<ChannelResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Array.Empty<ChannelResponse>();
        }
    }

    public async Task<UserSettingsResponse> GetUserSettings()
    {
        var response = await _client.GetAsync("users/settings");
        var userSettings = await response.Content.ReadFromJsonAsync<UserSettingsResponse>();
        return userSettings ?? new UserSettingsResponse();
    }

    public async Task<UserSettingsResponse> SaveUserSettings(UserSettingsRequest userSettingsRequest)
    {
        var response = await _client.PatchAsJsonAsync<UserSettingsRequest>("users/settings", userSettingsRequest);
        var userSettingsResponse = await response.Content.ReadFromJsonAsync<UserSettingsResponse>();
        return userSettingsResponse ?? new UserSettingsResponse();
    }

    public async Task<DailyMessageCountResponse[]> GetDailyMessageCount(int numOfDays = 10)
    {
        var response = await _client.GetFromJsonAsync<DailyMessageCountResponse[]>("MessageCounts/daily");
        return response ?? Array.Empty<DailyMessageCountResponse>();
    }

    public async Task SendMessage(SendMessageRequest sendMessageRequest)
    {
        await _client.PostAsJsonAsync("Messages/send", sendMessageRequest);
    }

    public async Task ChangePassword(ChangePasswordRequest changePasswordRequest)
    {
        await _client.PostAsJsonAsync("auth/change-password", changePasswordRequest);
    }

    public async Task<IdentityUserResponse[]> GetIdentityUsers()
        => await _client.GetFromJsonAsync<IdentityUserResponse[]>("Auth/users") ?? Array.Empty<IdentityUserResponse>();

}