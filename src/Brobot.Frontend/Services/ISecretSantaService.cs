using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public interface ISecretSantaService : IApiService<SecretSantaGroupRequest, SecretSantaGroupResponse, int>
{
    Task<SecretSantaGroupResponse> AddUserToGroup(int secretSantaGroupId, UserResponse user);
    Task RemoveUserFromGroup(int secretSantaGroupId, ulong userId);
    Task GeneratePairs(int secretSantaGroupId);
}