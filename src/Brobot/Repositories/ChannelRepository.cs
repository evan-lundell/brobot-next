using System.Diagnostics.CodeAnalysis;
using Brobot.Contexts;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public class ChannelRepository : RepositoryBase<ChannelModel, ulong>, IChannelRepository
{
    public ChannelRepository(BrobotDbContext context)
        : base(context)
    {
    }

    public override async Task Add(ChannelModel entity)
    {
        var existingChannel = await GetById(entity.Id);
        if (existingChannel is { Archived: true })
        {
            existingChannel.Archived = false;
            return;
        }

        if (existingChannel != null)
        {
            throw new ArgumentException($"Channel with id {entity.Id} already exists");
        }

        await base.Add(entity);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public override async Task AddRange(IEnumerable<ChannelModel> entities)
    {
        var channelIds = entities.Select((e) => e.Id);
        var existingChannels = await Find((c) => channelIds.Contains(c.Id));
        foreach (var existingChannel in existingChannels)
        {
            if (!existingChannel.Archived)
            {
                throw new ArgumentException($"Channel with id {existingChannel.Id} already exists");
            }

            existingChannel.Archived = false;
        }

        await base.AddRange(entities.ExceptBy(existingChannels.Select((ec) => ec.Id), (c) => c.Id));
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

    public async Task<IEnumerable<ChannelModel>> FindByUser(ulong userId)
        => await Context.Channels.FromSql(
                $"SELECT c.* FROM brobot.channel c INNER JOIN brobot.channel_user cu ON c.Id = cu.channel_id WHERE cu.user_id = {userId}")
            .ToListAsync();

    public Task<ChannelModel?> GetByIdWithChannelUsers(ulong channelId)
        => Context.Channels
            .Include((c) => c.ChannelUsers)
            .SingleOrDefaultAsync((c) => c.Id == channelId);
}