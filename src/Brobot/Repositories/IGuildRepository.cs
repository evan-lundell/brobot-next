using Brobot.Models;

namespace Brobot.Repositories;

public interface IGuildRepository : IRepository<GuildModel, ulong>
{
    IChannelRepository Channels { get; }
}