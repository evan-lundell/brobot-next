using Brobot.Models;

namespace Brobot.Repositories;

public interface IUserRepository : IRepository<DiscordUserModel, ulong>
{
    Task<IEnumerable<DiscordUserModel>> GetAllWithGuildsAndChannels();
    Task<DiscordUserModel?> GetByIdWithIncludes(ulong id);
}