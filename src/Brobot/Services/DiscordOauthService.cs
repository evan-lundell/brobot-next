using System.Text.Json;

namespace Brobot.Services;

public class DiscordOauthService(HttpClient client, IConfiguration configuration)
{
    public async Task<string> GetToken(string authorizationCode)
    {
        var body = new Dictionary<string, string>
        {
            { "client_id", configuration["DiscordClientId"] ?? "" },
            { "client_secret", configuration["DiscordClientSecret"] ?? "" },
            { "grant_type", "authorization_code" },
            { "code", authorizationCode }
        };

        var response = await client.PostAsync(configuration["DiscordTokenEndpoint"] ?? "", new FormUrlEncodedContent(body));
        var userData = await response.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = userData.GetProperty("access_token").GetString();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new Exception("Unable to get token");
        }
        return accessToken;
    }

    public async Task<ulong> GetDiscordUserId(string accessToken)
    {
        using var authenticatedClient = new HttpClient();
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        var response = await authenticatedClient.GetFromJsonAsync<JsonElement>(configuration["DiscordUserInformationEndpoint"] ?? "");
        var id = response.GetProperty("id").GetString();
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new Exception("Unable to get id");
        }
        return ulong.Parse(id);
    }
}