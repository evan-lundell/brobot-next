using System.Diagnostics.CodeAnalysis;
using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class ChannelRepository(BrobotDbContext context)
    : RepositoryBase<ChannelModel, ulong>(context), IChannelRepository
{
    public override async Task Add(ChannelModel entity, CancellationToken cancellationToken = default)
    {
        var existingChannel = await GetById(entity.Id, cancellationToken);
        if (existingChannel is { Archived: true })
        {
            existingChannel.Archived = false;
            return;
        }

        if (existingChannel != null)
        {
            throw new ArgumentException($"Channel with id {entity.Id} already exists");
        }

        await base.Add(entity, cancellationToken);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public override async Task AddRange(IEnumerable<ChannelModel> entities, CancellationToken cancellationToken = default)
    {
        var channelIds = entities.Select(e => e.Id);
        var existingChannels = await Find(c => channelIds.Contains(c.Id), cancellationToken);
        foreach (var existingChannel in existingChannels)
        {
            if (!existingChannel.Archived)
            {
                throw new ArgumentException($"Channel with id {existingChannel.Id} already exists");
            }

            existingChannel.Archived = false;
        }

        await base.AddRange(entities.ExceptBy(existingChannels.Select(ec => ec.Id), c => c.Id), cancellationToken);
    }

    public override void Remove(ChannelModel entity)
    {
        entity.Archived = true;
    }

    public override void RemoveRange(IEnumerable<ChannelModel> entities)
    {
        foreach (var channel in entities)
        {
            Remove(channel);
        }
    }

    public async Task<IEnumerable<ChannelModel>> FindByUser(ulong userId, CancellationToken cancellationToken = default)
        => await Context.Channels
            .Where(c => !c.Archived && c.ChannelUsers.Any(cu => cu.UserId == userId))
            .ToListAsync(cancellationToken);

    public Task<ChannelModel?> GetByIdWithChannelUsers(ulong channelId, CancellationToken cancellationToken = default)
        => Context.Channels
            .Include(c => c.ChannelUsers)
            .SingleOrDefaultAsync(c => c.Id == channelId, cancellationToken);
}
