using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class PlaylistSongRepository : RepositoryBase<PlaylistSongModel, int>, IPlaylistSongRepository
{
    public PlaylistSongRepository(BrobotDbContext brobotDbContext)
        : base(brobotDbContext)
    {
    }

    public override Task<PlaylistSongModel?> GetById(int id)
        => Context.PlaylistSongs
            .Include(ps => ps.Playlist)
            .SingleOrDefaultAsync(ps => ps.Id == id);
}