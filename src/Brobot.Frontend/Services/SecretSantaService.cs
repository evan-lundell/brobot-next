using System.Net.Http.Json;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;


public class SecretSantaService(HttpClient httpClient)
    : ApiServiceBase<SecretSantaGroupRequest, SecretSantaGroupResponse, int>("api/SecretSantaGroups",
        "SecretSantaGroup", httpClient), ISecretSantaService
{
    public async Task<SecretSantaGroupResponse> AddUserToGroup(int secretSantaGroupId, DiscordUserResponse discordUser)
    {
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/{secretSantaGroupId}/members", discordUser);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SecretSantaGroupResponse>()
                   ?? throw new Exception($"Failed to parse entity {EntityName}");
        }
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(errorResponse?.Title ?? "Failed to add user");
    }

    public async Task RemoveUserFromGroup(int secretSantaGroupId, ulong userId)
    {
        var response = await HttpClient.DeleteAsync($"{BaseUrl}/{secretSantaGroupId}/members/{userId}");
        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(errorResponse?.Title ?? "Failed to remove user");
        }
    }

    public async Task GeneratePairs(int secretSantaGroupId)
    {
        var response = await HttpClient.PostAsync($"{BaseUrl}/{secretSantaGroupId}/pairings", null);
        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(errorResponse?.Title ?? "Failed to generate pairs");
        }
    }
}