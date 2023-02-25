using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Brobot.Services;

public class DiscordEventHandler : IDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _services;
    private readonly ISyncService _sync;
    private readonly InteractionService _commands;
    private readonly HotOpService _hotOpService;

    public DiscordEventHandler(
        DiscordSocketClient client,
        IServiceProvider services,
        ISyncService sync,
        HotOpService hotOpService
    )
    {
        _client = client;
        _services = services;
        _sync = sync;
        _commands = new InteractionService(client);
        _hotOpService = hotOpService;
    }

    public void Start()
    {
        _client.Log += Log;
        _client.Ready += Ready;
        _client.InteractionCreated += InteractionCreated;
        _client.GuildAvailable += GuildAvailable;
        _client.GuildUnavailable += GuildUnavailable;
        _client.GuildUpdated += GuildUpdated;
        _client.ChannelCreated += ChannelCreated;
        _client.ChannelDestroyed += ChannelDestroyed;
        _client.ChannelUpdated += ChannelUpdated;
        _client.MessageReceived += MessageReceived;
        _client.MessageDeleted += MessageDeleted;
        _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
        _client.PresenceUpdated += PresenceUpdated;
    }

    public void Dispose()
    {
        _client.Log -= Log;
        _client.Ready -= Ready;
        _client.InteractionCreated -= InteractionCreated;
        _client.GuildAvailable -= GuildAvailable;
        _client.GuildUnavailable -= GuildUnavailable;
        _client.ChannelCreated -= ChannelCreated;
        _client.ChannelDestroyed -= ChannelDestroyed;
        _client.ChannelUpdated -= ChannelUpdated;
        _client.MessageReceived -= MessageReceived;
        _client.MessageDeleted -= MessageDeleted;
        _client.PresenceUpdated -= PresenceUpdated;
    }

    private Task PresenceUpdated(SocketUser socketUser, SocketPresence formerPresence, SocketPresence currentPresence)
    {
        return _sync.PresenceUpdated(socketUser, formerPresence, currentPresence);
    }

    private Task UserVoiceStateUpdated(
        SocketUser socketUser,
        SocketVoiceState previousVoiceState,
        SocketVoiceState currentVoiceState)
    {
        return _hotOpService.UserVoiceStateUpdated(socketUser, previousVoiceState, currentVoiceState);
    }

    private Task MessageReceived(SocketMessage socketMessage)
    {
        return Task.Run(async () =>
        {
            if (socketMessage.Content.ToLower() == "good bot")
            {
                await socketMessage.Channel.SendMessageAsync("Thanks! :robot:");
                return;
            }

            if (socketMessage.Content.ToLower() == "bad bot")
            {
                await socketMessage.Channel.SendMessageAsync(":middle_finger:");
                return;
            }
        });
    }

    private Task MessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        return Task.Run(async () =>
        {
            var now = DateTimeOffset.UtcNow;
            var channel = await cachedChannel.GetOrDownloadAsync();
            var message = await cachedMessage.GetOrDownloadAsync();
            if (message == null)
            {
                message = await channel.GetMessageAsync(cachedMessage.Id);
            }
            if (!(channel is SocketTextChannel textChannel) || channel is SocketVoiceChannel)
            {
                return;
            }

            var auditLog = (await textChannel.Guild.GetAuditLogsAsync(limit: 1, actionType: ActionType.MessageDeleted).FlattenAsync()).First();
            if (auditLog == null)
            {
                return;
            }

            var username = "";
            if (now - auditLog.CreatedAt < TimeSpan.FromSeconds(5))
            {
                username = auditLog.User.Username;
            }
            else
            {
                username = message.Author.Username;
            }

            await textChannel.SendMessageAsync($"I saw that {username} :spy:");
        });
    }

    private Task GuildUpdated(SocketGuild previous, SocketGuild current)
    {
        return _sync.GuildUpdated(previous, current);
    }

    private Task ChannelUpdated(SocketChannel previous, SocketChannel current)
    {
        var tasks = new List<Task>();
        if (!(previous is SocketTextChannel previousTextChannel)
            || previous is SocketVoiceChannel
            || !(current is SocketTextChannel currentTextChannel)
            || current is SocketVoiceChannel)
        {
            return Task.CompletedTask;
        }

        return _sync.ChannelUpdated(previousTextChannel, currentTextChannel);
    }

    private Task ChannelDestroyed(SocketChannel channel)
    {
        if (!(channel is SocketTextChannel textChannel) || channel is SocketVoiceChannel)
        {
            return Task.CompletedTask;
        }
        return _sync.ChannelDestroyed(textChannel);
    }

    private Task ChannelCreated(SocketChannel channel)
    {
        if (!(channel is SocketTextChannel textChannel) || channel is SocketVoiceChannel)
        {
            return Task.CompletedTask;
        }
        return _sync.ChannelCreated(textChannel);
    }

    private Task GuildUnavailable(SocketGuild guild)
    {
        return _sync.GuildUnavailable(guild);
    }

    private Task GuildAvailable(SocketGuild guild)
    {
        return _sync.GuildAvailable(guild);
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        var ctx = new SocketInteractionContext(_client, interaction);
        await _commands.ExecuteCommandAsync(ctx, _services);
    }

    private async Task Ready()
    {

        using (var scope = _services.CreateScope())
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), scope.ServiceProvider);

        }

        _ = _commands.RegisterCommandsGloballyAsync();
        _ = _sync.SyncOnStartup();
    }

    private Task Log(LogMessage logMessage)
    {
        Console.WriteLine(logMessage.Message);
        if (logMessage.Exception != null)
        {
            Console.WriteLine(logMessage.Exception.Message);
        }
        return Task.CompletedTask;
    }
}