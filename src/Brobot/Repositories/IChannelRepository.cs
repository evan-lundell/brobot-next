using Brobot.Models;

namespace Brobot.Repositories;

public interface IChannelRepository : IRepository<ChannelModel, ulong>
{
    Task<IEnumerable<ChannelModel>> FindByUser(ulong userId, CancellationToken cancellationToken = default);
    Task<ChannelModel?> GetByIdWithChannelUsers(ulong channelId, CancellationToken cancellationToken = default);
}