using Brobot.Models;

namespace Brobot.Repositories;

public interface IUserRepository : IRepository<DiscordUserModel, ulong>
{
    Task<IEnumerable<DiscordUserModel>> GetAllWithGuildsAndChannels(CancellationToken cancellationToken = default);
    Task<DiscordUserModel?> GetByIdWithIncludes(ulong id, CancellationToken cancellationToken = default);
}