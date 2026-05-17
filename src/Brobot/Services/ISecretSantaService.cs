using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Services;

public interface ISecretSantaService
{
    Task<IEnumerable<SecretSantaGroupResponse>> GetSecretSantaGroups(CancellationToken cancellationToken = default);
    Task<SecretSantaGroupResponse?> GetSecretSantaGroup(int secretSantaGroupId, CancellationToken cancellationToken = default);
    Task<SecretSantaGroupResponse> CreateSecretSantaGroup(SecretSantaGroupRequest secretSantaGroup, CancellationToken cancellationToken = default);
    Task<SecretSantaGroupResponse> AddUserToGroup(int secretSantaGroupId, DiscordUserResponse discordUser, CancellationToken cancellationToken = default);
    Task<SecretSantaGroupResponse> RemoveUserFromGroup(int secretSantaGroupId, ulong userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SecretSantaPairResponse>> GeneratePairsForCurrentYear(int secretSantaGroupId, CancellationToken cancellationToken = default);
    Task SendPairs(IEnumerable<SecretSantaPairResponse> pairs, CancellationToken cancellationToken = default);
}