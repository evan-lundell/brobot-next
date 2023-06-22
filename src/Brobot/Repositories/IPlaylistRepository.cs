using Brobot.Models;

namespace Brobot.Repositories;

public interface IPlaylistRepository : IRepository<PlaylistModel, int>
{
    Task<IEnumerable<PlaylistModel>> GetPlaylistsFromUser(ulong userId);
}