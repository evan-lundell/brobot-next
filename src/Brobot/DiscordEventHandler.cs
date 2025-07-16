using System.Reflection;
using Brobot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

// ReSharper disable MemberCanBeMadeStatic.Local

namespace Brobot;

public class DiscordEventHandler : IDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly ILogger<DiscordEventHandler> _logger;
    private readonly ISyncService _syncService;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;

    public DiscordEventHandler(DiscordSocketClient client, IServiceProvider services)
    {
        _client = client;
        _commands = new InteractionService(_client);
        _logger = services.GetRequiredService<ILogger<DiscordEventHandler>>();
        _syncService =  services.GetRequiredService<ISyncService>();
        _config = services.GetRequiredService<IConfiguration>();
        _services = services;
        
    }

    public void RegisterEvents()
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
        _client.ThreadCreated += ThreadCreated;
        _client.ThreadDeleted += ThreadDeleted;
        _client.ThreadUpdated += ThreadUpdated;
        _client.ThreadMemberJoined += ThreadMemberJoined;
        _client.ThreadMemberLeft += ThreadMemberLeft;
    }

#pragma warning disable CA1816
    public void Dispose()
#pragma warning restore CA1816
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
        _client.ThreadCreated -= ThreadCreated;
        _client.ThreadDeleted -= ThreadDeleted;
        _client.ThreadUpdated -= ThreadUpdated;
        _client.ThreadMemberJoined -= ThreadMemberJoined;
        _client.ThreadMemberLeft -= ThreadMemberLeft;
    }

    private Task PresenceUpdated(SocketUser socketUser, SocketPresence formerPresence, SocketPresence currentPresence)
    {
        _syncService.PresenceUpdated(socketUser, formerPresence, currentPresence);
        return Task.CompletedTask;
    }

    private Task UserVoiceStateUpdated(
        SocketUser socketUser,
        SocketVoiceState previousVoiceState,
        SocketVoiceState currentVoiceState)
    {
        if ((previousVoiceState.VoiceChannel == null && currentVoiceState.VoiceChannel == null)
            || socketUser.IsBot)
        {
            return Task.CompletedTask;
        }
        _ = _syncService.UserVoiceStateUpdated(socketUser, previousVoiceState, currentVoiceState);
        return Task.CompletedTask;
    }

    private Task MessageReceived(SocketMessage socketMessage)
    {
        _ = _syncService.MessageReceived(socketMessage);
        return Task.CompletedTask;
    }

    private Task MessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        _ = Task.Run(async () =>
        {
            var channel = await cachedChannel.GetOrDownloadAsync();
            var message = await cachedMessage.GetOrDownloadAsync() ??
                          await channel.GetMessageAsync(cachedMessage.Id);
            if (channel is IGuildChannel guildChannel)
            {
                await _syncService.MessageDeleted(message, channel, guildChannel.Guild);
            }
        });
        return Task.CompletedTask;
    }

    private Task GuildUpdated(SocketGuild previous, SocketGuild current)
    {
        _ = _syncService.GuildUpdated(previous, current);
        return Task.CompletedTask;
    }

    private Task ChannelUpdated(SocketChannel previous, SocketChannel current)
    {
        if (previous is not SocketTextChannel previousTextChannel
            || previous is SocketVoiceChannel
            || current is not SocketTextChannel currentTextChannel
            || current is SocketVoiceChannel)
        {
            return Task.CompletedTask;
        }

        var guild = _client.GetGuild(previousTextChannel.Guild.Id);
        _ = _syncService.ChannelUpdated(guild, previousTextChannel, currentTextChannel);
        return Task.CompletedTask;
    }

    private Task ChannelDestroyed(SocketChannel channel)
    {
        if (channel is not SocketTextChannel textChannel || channel is SocketVoiceChannel)
        {
            return Task.CompletedTask;
        }
        _ = _syncService.ChannelDestroyed(textChannel);
        return Task.CompletedTask;
    }

    private Task ChannelCreated(SocketChannel channel)
    {
        if (channel is not SocketTextChannel textChannel || channel is SocketVoiceChannel)
        {
            return Task.CompletedTask;
        }
        _ = _syncService.ChannelCreated(textChannel);
        return Task.CompletedTask;
    }

    private Task GuildUnavailable(SocketGuild guild)
    {
        _ = _syncService.GuildUnavailable(guild);
        return Task.CompletedTask;
    }

    private Task GuildAvailable(SocketGuild guild)
    {
        _ = _syncService.GuildAvailable(guild);
        return Task.CompletedTask;
    }

    private Task InteractionCreated(SocketInteraction interaction)
    {
        _ = Task.Run(async () =>
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _commands.ExecuteCommandAsync(ctx, _services);
        });
        return Task.CompletedTask;
    }

    private async Task Ready()
    {

        using (var scope = _services.CreateScope())
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), scope.ServiceProvider);
        }

        if (_config["DOTNET_ENVIRONMENT"] == "Development")
        {
            _ = _commands.RegisterCommandsToGuildAsync(421404457599762433);
        }
        else
        {
            _ = _commands.RegisterCommandsGloballyAsync();
        }

        if (!bool.TryParse(_config["NoSync"], out bool noSync) || !noSync)
        {
            _ = _syncService.SyncOnStartup();
        }
    }

    private Task Log(LogMessage logMessage)
    {
        if (logMessage.Exception != null)
        {
            _logger.LogError(logMessage.Exception, "{LogMessageSource}: {LogMessageMessage}", logMessage.Source, logMessage.Message);
        }
        else
        {
            _logger.LogInformation("{LogMessageSource}: {LogMessageMessage}",  logMessage.Source, logMessage.Message);
        }
        return Task.CompletedTask;
    }

    private Task ThreadCreated(SocketThreadChannel thread)
    {
        _ = _syncService.ThreadCreated(thread);
        return Task.CompletedTask;
    }

    private Task ThreadDeleted(Cacheable<SocketThreadChannel, ulong> thread)
    {
        _ = Task.Run(async () =>
        {
            var threadChannel = await thread.GetOrDownloadAsync();
            await _syncService.ThreadDeleted(threadChannel);
        });
        return Task.CompletedTask;
    }
    
    private Task ThreadUpdated(Cacheable<SocketThreadChannel, ulong> oldThreadChannelCacheable, SocketThreadChannel newThreadChannel)
    {
        _ = Task.Run(async () =>
        {
            var oldThreadChannel = await oldThreadChannelCacheable.GetOrDownloadAsync();
            if (oldThreadChannel == null)
            {
                return;
            }
            await _syncService.ThreadUpdated(oldThreadChannel, newThreadChannel);
        });
        
        return Task.CompletedTask;
    }
    
    private Task ThreadMemberJoined(SocketThreadUser threadUser)
    {
        _ = _syncService.ThreadMemberJoined(threadUser);
        return Task.CompletedTask;
    }
    
    private Task ThreadMemberLeft(SocketThreadUser threadUser)
    {
        _ = _syncService.ThreadMemberLeft(threadUser);
        return Task.CompletedTask;
    }
}