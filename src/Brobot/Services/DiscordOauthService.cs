using System.Text.Json;
using Brobot.Configuration;
using Microsoft.Extensions.Options;

namespace Brobot.Services;

public class DiscordOauthService(HttpClient client, IOptions<DiscordOptions> discordOptions, ILogger<DiscordOauthService> logger)
{
    public async Task<string> GetToken(string authorizationCode)
    {
        logger.LogInformation("Getting token");
        var body = new Dictionary<string, string>
        {
            { "client_id", discordOptions.Value.ClientId },
            { "client_secret", discordOptions.Value.ClientSecret },
            { "grant_type", "authorization_code" },
            { "code", authorizationCode }
        };

        var response = await client.PostAsync(discordOptions.Value.TokenEndpoint, new FormUrlEncodedContent(body));
        var userData = await response.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = userData.GetProperty("access_token").GetString();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            logger.LogError("Unable to get access_token");
            throw new Exception("Unable to get token");
        }
        logger.LogInformation("Obtained access token");
        return accessToken;
    }

    public async Task<ulong> GetDiscordUserId(string accessToken)
    {
        logger.LogInformation("Getting user id");
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        var response = await client.GetFromJsonAsync<JsonElement>(discordOptions.Value.UserInformationEndpoint);
        var id = response.GetProperty("id").GetString();
        if (string.IsNullOrWhiteSpace(id))
        {
            logger.LogError("Unable to get id");
            throw new Exception("Unable to get id");
        }
        logger.LogInformation("Obtained user id");
        return ulong.Parse(id);
    }
}