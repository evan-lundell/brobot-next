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
}