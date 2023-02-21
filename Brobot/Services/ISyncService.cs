using Discord.WebSocket;

namespace Brobot.Services;

public interface ISyncService
{
    Task SyncOnStartup();
    Task GuildAvailable(SocketGuild guild);
    Task GuildUnavailable(SocketGuild guild);
    Task GuildUpdated(SocketGuild previousGuild, SocketGuild currentGuild);
    Task ChannelCreated(SocketTextChannel channel);
    Task ChannelDestroyed(SocketTextChannel channel);
    Task ChannelUpdated(SocketTextChannel previous, SocketTextChannel current);
}