using Discord;
using Discord.WebSocket;

namespace Brobot.Services;

public interface ISyncService
{
    Task SyncOnStartup();
    Task GuildAvailable(IGuild guild);
    Task GuildUnavailable(IGuild guild);
    Task GuildUpdated(IGuild previousGuild, IGuild currentGuild);
    Task ChannelCreated(IGuildChannel channel);
    Task ChannelDestroyed(IGuildChannel channel);
    Task ChannelUpdated(IGuild guild, ISocketMessageChannel previous, ISocketMessageChannel current);
    Task PresenceUpdated(IUser socketUser, IPresence formerSocketPresence, IPresence currentSocketPresence);
    Task UserVoiceStateUpdated(IUser user, IVoiceState previousVoiceState, IVoiceState currentVoiceState);
    Task MessageReceived(IMessage message);
    Task MessageDeleted(IMessage message, IMessageChannel channel, IGuild guild);
    Task ThreadCreated(IThreadChannel thread);
    Task ThreadDeleted(IThreadChannel thread);
    Task ThreadMemberJoined(IThreadUser user);
    Task ThreadUpdated(IThreadChannel oldThread, IThreadChannel newThread);
    Task ThreadMemberLeft(IThreadUser threadUser);
}