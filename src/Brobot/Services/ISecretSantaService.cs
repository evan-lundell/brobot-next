using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Services;

public interface ISecretSantaService
{
    Task<IEnumerable<SecretSantaGroupResponse>> GetSecretSantaGroups();
    Task<SecretSantaGroupResponse?> GetSecretSantaGroup(int secretSantaGroupId);
    Task<SecretSantaGroupResponse> CreateSecretSantaGroup(SecretSantaGroupRequest secretSantaGroup);
    Task<SecretSantaGroupResponse> AddUserToGroup(int secretSantaGroupId, UserResponse user);
    Task<SecretSantaGroupResponse> RemoveUserFromGroup(int secretSantaGroupId, ulong userId);
    Task<IEnumerable<SecretSantaPairResponse>> GeneratePairsForCurrentYear(int secretSantaGroupId);
    Task SendPairs(IEnumerable<SecretSantaPairResponse> pairs);
}