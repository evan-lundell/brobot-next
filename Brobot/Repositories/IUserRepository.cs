using Brobot.Models;

namespace Brobot.Repositories;

public interface IUserRepository : IRepository<UserModel, ulong>
{
    Task<IEnumerable<UserModel>> GetAllWithGuildsAndChannels();
    Task<UserModel?> GetByIdWithIncludes(ulong id);
    Task<UserModel?> GetFromIdentityUserId(string identityUserId);
}