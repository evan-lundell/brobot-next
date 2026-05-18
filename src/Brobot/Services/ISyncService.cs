using Discord;
using Discord.WebSocket;

namespace Brobot.Services;

public interface ISyncService
{
    Task SyncOnStartup(CancellationToken cancellationToken = default);
    Task GuildAvailable(IGuild guild, CancellationToken cancellationToken = default);
    Task GuildUnavailable(IGuild guild, CancellationToken cancellationToken = default);
    Task GuildUpdated(IGuild previousGuild, IGuild currentGuild, CancellationToken cancellationToken = default);
    Task ChannelCreated(IGuildChannel channel, CancellationToken cancellationToken = default);
    Task ChannelDestroyed(IGuildChannel channel, CancellationToken cancellationToken = default);
    Task ChannelUpdated(IGuild guild, ISocketMessageChannel previous, ISocketMessageChannel current, CancellationToken cancellationToken = default);
    Task PresenceUpdated(IUser socketUser, IPresence formerSocketPresence, IPresence currentSocketPresence, CancellationToken cancellationToken = default);
    Task UserVoiceStateUpdated(IUser user, IVoiceState previousVoiceState, IVoiceState currentVoiceState, CancellationToken cancellationToken = default);
    Task MessageReceived(IMessage message, CancellationToken cancellationToken = default);
    Task MessageDeleted(IMessage message, IMessageChannel channel, IGuild guild, CancellationToken cancellationToken = default);
    Task ThreadCreated(IThreadChannel thread, CancellationToken cancellationToken = default);
    Task ThreadDeleted(IThreadChannel thread, CancellationToken cancellationToken = default);
    Task ThreadMemberJoined(IThreadUser user, CancellationToken cancellationToken = default);
    Task ThreadUpdated(IThreadChannel oldThread, IThreadChannel newThread, CancellationToken cancellationToken = default);
    Task ThreadMemberLeft(IThreadUser threadUser, CancellationToken cancellationToken = default);
}