using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class UserRepository(BrobotDbContext context) : RepositoryBase<DiscordUserModel, ulong>(context), IUserRepository
{
    public override async Task Add(DiscordUserModel entity, CancellationToken cancellationToken = default)
    {
        var existingUser = await GetById(entity.Id, cancellationToken);
        if (existingUser is { Archived: true })
        {
            existingUser.Archived = false;
            return;
        }

        if (existingUser != null)
        {
            throw new ArgumentException($"User with ID of {entity.Id} already exists");
        }

        await base.Add(entity, cancellationToken);
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

    public async Task<IEnumerable<DiscordUserModel>> GetAllWithGuildsAndChannels(CancellationToken cancellationToken = default)
    {
        return await Context.DiscordUsers
            .AsSplitQuery()
            .Include(u => u.GuildUsers)
            .ThenInclude(gu => gu.Guild)
            .Include(u => u.ChannelUsers)
            .ThenInclude(cu => cu.Channel)
            .Include(u => u.ScheduledMessages)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public Task<DiscordUserModel?> GetByIdWithIncludes(ulong id, CancellationToken cancellationToken = default)
    {
        return Context.DiscordUsers
            .AsSplitQuery()
            .Include(u => u.GuildUsers)
            .ThenInclude(gu => gu.Guild)
            .Include(u => u.ChannelUsers)
            .ThenInclude(cu => cu.Channel)
            .Include(u => u.ScheduledMessages)
            .AsSplitQuery()
            .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}