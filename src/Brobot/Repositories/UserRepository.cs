using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class UserRepository(BrobotDbContext context) : RepositoryBase<DiscordUserModel, ulong>(context), IUserRepository
{
    public override async Task Add(DiscordUserModel entity)
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

    public override void Remove(DiscordUserModel entity)
    {
        entity.Archived = true;
        entity.GuildUsers.Clear();
        entity.ChannelUsers.Clear();
    }

    public override void RemoveRange(IEnumerable<DiscordUserModel> entities)
    {
        foreach (var user in entities)
        {
            Remove(user);
        }
    }

    public async Task<IEnumerable<DiscordUserModel>> GetAllWithGuildsAndChannels()
    {
        return await Context.DiscordUsers
            .AsSplitQuery()
            .Include(u => u.GuildUsers)
            .ThenInclude(gu => gu.Guild)
            .Include(u => u.ChannelUsers)
            .ThenInclude(cu => cu.Channel)
            .Include(u => u.ScheduledMessages)
            .AsSplitQuery()
            .ToListAsync();
    }

    public Task<DiscordUserModel?> GetByIdWithIncludes(ulong id)
    {
        return Context.DiscordUsers
            .AsSplitQuery()
            .Include(u => u.GuildUsers)
            .ThenInclude(gu => gu.Guild)
            .Include(u => u.ChannelUsers)
            .ThenInclude(cu => cu.Channel)
            .Include(u => u.ScheduledMessages)
            .AsSplitQuery()
            .SingleOrDefaultAsync(u => u.Id == id);
    }
}