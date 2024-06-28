using System.Linq.Expressions;
using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class PlaylistRepository : RepositoryBase<PlaylistModel, int>, IPlaylistRepository
{
    public PlaylistRepository(BrobotDbContext brobotDbContext)
        : base(brobotDbContext)
    {
    }

    public override async Task<IEnumerable<PlaylistModel>> GetAll()
    {
        var playlists = await Context.Playlists
            .Include(p => p.Songs)
            .ToListAsync();
        return playlists;
    }

    public override Task<PlaylistModel?> GetById(int id)
    {
        return Context.Playlists
            .Include(p => p.Songs)
            .SingleOrDefaultAsync(p => p.Id == id);
    }

    public override async Task<IEnumerable<PlaylistModel>> Find(Expression<Func<PlaylistModel, bool>> expression)
    {
        var playlists = await Context.Playlists
            .Include(p => p.Songs)
            .Where(expression)
            .ToListAsync();
        return playlists;
    }

    public async Task<IEnumerable<PlaylistModel>> GetPlaylistsFromUser(ulong userId)
        => await Context.Playlists
            .AsNoTracking()
            .Include(p => p.Songs)
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .ToListAsync();

}