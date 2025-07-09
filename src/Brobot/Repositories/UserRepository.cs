using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class UserRepository(BrobotDbContext context) : RepositoryBase<UserModel, ulong>(context), IUserRepository
{
    public override async Task Add(UserModel entity)
    {
        var existingUser = await GetById(entity.Id);
        if (existingUser is { Archived: true })
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
        return await Context.Users
            .Include(u => u.GuildUsers)
            .ThenInclude(gu => gu.Guild)
            .Include(u => u.ChannelUsers)
            .ThenInclude(cu => cu.Channel)
            .Include(u => u.ScheduledMessages)
            .AsSplitQuery()
            .ToListAsync();
    }

    public Task<UserModel?> GetByIdWithIncludes(ulong id)
    {
        return Context.Users
            .Include(u => u.GuildUsers)
            .ThenInclude(gu => gu.Guild)
            .Include(u => u.ChannelUsers)
            .ThenInclude(cu => cu.Channel)
            .Include(u => u.ScheduledMessages)
            .AsSplitQuery()
            .SingleOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserModel?> GetFromIdentityUserId(string identityUserId)
    {
        var user = await Context.Users
            .SingleOrDefaultAsync(u => u.IdentityUserId == identityUserId);
        return user;
    }

    public async Task<IEnumerable<UserModel>> GetUsersFromIdentityUserIds(IEnumerable<string> identityUserIds)
    {
        return await Context.Users
            .Where(u => identityUserIds.Contains(u.IdentityUserId))
            .ToListAsync();
    }
}