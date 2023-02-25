using Microsoft.EntityFrameworkCore;
using Brobot.Contexts;
using Brobot.Models;

namespace Brobot.Repositories;

public class UserRepository : RepositoryBase<UserModel, ulong>, IUserRepository
{
    public UserRepository(BrobotDbContext context)
        : base(context)
    {
    }

    public async override Task Add(UserModel entity)
    {
        var existingUser = await GetById(entity.Id);
        if (existingUser != null && existingUser.Archived)
        {
            existingUser.Archived = false;
            return;
        }

        if (existingUser != null)
        {
            throw new ArgumentException($"User with ID of {entity.Id} already exists");
        }

        await base.Add(entity);
    }

    public override void Remove(UserModel entity)
    {
        entity.Archived = true;
        entity.GuildUsers.Clear();
        entity.ChannelUsers.Clear();
    }

    public override void RemoveRange(IEnumerable<UserModel> entities)
    {
        foreach (var user in entities)
        {
            Remove(user);
        }
    }

    public async Task<IEnumerable<UserModel>> GetAllWithGuildsAndChannels()
    {
        return await _context.Users
            .Include((u) => u.GuildUsers)
            .ThenInclude((gu) => gu.Guild)
            .Include((u) => u.ChannelUsers)
            .ThenInclude((cu) => cu.Channel)
            .Include((u) => u.ScheduledMessages)
            .AsSplitQuery()
            .ToListAsync();
    }

    public Task<UserModel?> GetByIdWithIncludes(ulong id)
    {
        return _context.Users
            .Include((u) => u.GuildUsers)
            .ThenInclude((gu) => gu.Guild)
            .Include((u) => u.ChannelUsers)
            .ThenInclude((cu) => cu.Channel)
            .Include((u) => u.ScheduledMessages)
            .AsSplitQuery()
            .SingleOrDefaultAsync((u) => u.Id == id);
    }
}