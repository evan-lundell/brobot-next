using Brobot.Models;

namespace Brobot.Repositories;

public interface IUserRepository : IRepository<UserModel, ulong>
{
    Task<IEnumerable<UserModel>> GetAllWithIncludes();
    Task<UserModel?> GetByIdWithIncludes(ulong id);
}