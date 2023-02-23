using Brobot.Contexts;
using Brobot.Models;

namespace Brobot.Repositories;

public class ChannelRepository : RepositoryBase<ChannelModel, ulong>, IChannelRepository
{
    public ChannelRepository(BrobotDbContext context)
        : base(context)
    {
    }

    public async override Task Add(ChannelModel entity)
    {
        var existingChannel = await GetById(entity.Id);
        if (existingChannel != null && existingChannel.Archived)
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

    public async override Task AddRange(IEnumerable<ChannelModel> entities)
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
        entity.ChannelUsers.Clear();
    }

    public override void RemoveRange(IEnumerable<ChannelModel> entities)
    {
        foreach (var channel in entities)
        {
            Remove(channel);
        }
    }
}