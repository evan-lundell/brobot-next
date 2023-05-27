using System.Diagnostics.CodeAnalysis;
using Brobot.Contexts;
using Brobot.Models;

namespace Brobot.Repositories;

public class GuildRepository : RepositoryBase<GuildModel, ulong>, IGuildRepository
{
    public IChannelRepository Channels { get; }
    public GuildRepository(BrobotDbContext context, IChannelRepository channels)
        : base(context)
    {
        Channels = channels;
    }

    public override async Task Add(GuildModel entity)
    {
        var guild = await GetById(entity.Id);
        if (guild is { Archived: true })
        {
            guild.Archived = false;
            return;
        }

        if (guild != null)
        {
            throw new ArgumentException($"Guild with id {entity.Id} already exists");
        }

        await base.Add(entity);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public override async Task AddRange(IEnumerable<GuildModel> entities)
    {
        var guildIds = entities.Select((e) => e.Id);
        var existingGuilds = await Find((g) => guildIds.Contains(g.Id));
        foreach (var existingGuild in existingGuilds)
        {
            if (!existingGuild.Archived)
            {
                throw new ArgumentException($"Guild with id {existingGuild.Id} already exists");
            }
            existingGuild.Archived = false;
        }
        await base.AddRange(entities.ExceptBy(existingGuilds.Select(eg => eg.Id), (e) => e.Id));
    }

    public override void Remove(GuildModel entity)
    {
        entity.Archived = true;
        entity.GuildUsers.Clear();
        Channels.RemoveRange(entity.Channels);
    }

    public override void RemoveRange(IEnumerable<GuildModel> entities)
    {
        foreach (var guild in entities)
        {
            Remove(guild);
        }
    }
}