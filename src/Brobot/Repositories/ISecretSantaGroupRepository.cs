using Brobot.Models;

namespace Brobot.Repositories;

public interface ISecretSantaGroupRepository : IRepository<SecretSantaGroupModel, int>
{
    Task<IEnumerable<SecretSantaPairModel>> GetPairs(int secretSantaGroupId, int year);
}