using System.Reflection;
using Brobot.Services;
using Brobot.TaskQueue;
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
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    private int _readyInitializationQueued = 0;

    public DiscordEventHandler(DiscordSocketClient client, IServiceProvider services)
    {
        _client = client;
        _commands = new InteractionService(_client);
        _logger = services.GetRequiredService<ILogger<DiscordEventHandler>>();
        _syncService =  services.GetRequiredService<ISyncService>();
        _config = services.GetRequiredService<IConfiguration>();
        _backgroundTaskQueue = services.GetRequiredService<IBackgroundTaskQueue>();
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
        _client.GuildUpdated -= GuildUpdated;
        _client.ChannelCreated -= ChannelCreated;
        _client.ChannelDestroyed -= ChannelDestroyed;
        _client.ChannelUpdated -= ChannelUpdated;
        _client.MessageReceived -= MessageReceived;
        _client.MessageDeleted -= MessageDeleted;
        _client.UserVoiceStateUpdated -= UserVoiceStateUpdated;
        _client.PresenceUpdated -= PresenceUpdated;
        _client.ThreadCreated -= ThreadCreated;
        _client.ThreadDeleted -= ThreadDeleted;
        _client.ThreadUpdated -= ThreadUpdated;
        _client.ThreadMemberJoined -= ThreadMemberJoined;
        _client.ThreadMemberLeft -= ThreadMemberLeft;
    }

    private Task PresenceUpdated(SocketUser socketUser, SocketPresence formerPresence, SocketPresence currentPresence)
    {
        if (currentPresence.Status == UserStatus.Online || socketUser.IsBot)
        {
            return Task.CompletedTask;
        }

        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            await _syncService.PresenceUpdated(socketUser, formerPresence, currentPresence, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue presence updated for {UserId}", socketUser.Id);
        }
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

        if (previousVoiceState.VoiceChannel?.Id == currentVoiceState.VoiceChannel?.Id)
        {
            _logger.LogInformation("No channel change, finished processing user voice state updated for {UserId}", socketUser.Id);
            return Task.CompletedTask;
        }
        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            await _syncService.UserVoiceStateUpdated(socketUser, previousVoiceState, currentVoiceState, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue user voice state updated for {UserId}", socketUser.Id);
        }
        return Task.CompletedTask;
    }

    private Task MessageReceived(SocketMessage socketMessage)
    {
        var queued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            await _syncService.MessageReceived(socketMessage, ct);
        });
        if (!queued)
        {
            _logger.LogWarning("Failed to queue message received for {MessageId}", socketMessage.Id);
        }
        return Task.CompletedTask;
    }

    private Task MessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }
            var channel = await cachedChannel.GetOrDownloadAsync();
            if (channel == null)
            {
                _logger.LogDebug("MessageDeleted: channel {ChannelId} unavailable", cachedChannel.Id);
                return;
            }

            var message = await cachedMessage.GetOrDownloadAsync()
                        ?? await channel.GetMessageAsync(cachedMessage.Id);
            if (message == null)
            {
                _logger.LogDebug(
                    "MessageDeleted: message {MessageId} unavailable in channel {ChannelId}",
                    cachedMessage.Id, channel.Id);
                return;
            }

            if (channel is not IGuildChannel guildChannel)
            {
                return; // DM/group channel, nothing to sync
            }

            await _syncService.MessageDeleted(message, channel, guildChannel.Guild, ct);
        });

        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue message deleted for {MessageId}", cachedMessage.Id);
        }
        return Task.CompletedTask;
    }

    private Task GuildUpdated(SocketGuild previous, SocketGuild current)
    {
        if (previous.Name == current.Name)
        {
            return Task.CompletedTask;
        }

        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            await _syncService.GuildUpdated(previous, current, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue guild updated for {GuildId}", previous.Id);
        }
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
        if (guild == null)
        {
            return Task.CompletedTask;
        }
        
        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            await _syncService.ChannelUpdated(guild, previousTextChannel, currentTextChannel, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue channel updated for {ChannelId}", previousTextChannel.Id);
        }
        return Task.CompletedTask;
    }

    private Task ChannelDestroyed(SocketChannel channel)
    {
        if (channel is not SocketTextChannel textChannel || channel is SocketVoiceChannel)
        {
            return Task.CompletedTask;
        }
        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            await _syncService.ChannelDestroyed(textChannel, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue channel destroyed for {ChannelId}", textChannel.Id);
        }
        return Task.CompletedTask;
    }

    private Task ChannelCreated(SocketChannel channel)
    {
        if (channel is not SocketTextChannel textChannel || channel is SocketVoiceChannel)
        {
            return Task.CompletedTask;
        }
        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            await _syncService.ChannelCreated(textChannel, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue channel created for {ChannelId}", textChannel.Id);
        }
        return Task.CompletedTask;
    }

    private Task GuildUnavailable(SocketGuild guild)
    {
        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            await _syncService.GuildUnavailable(guild, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue guild unavailable for {GuildId}", guild.Id);
        }
        return Task.CompletedTask;
    }

    private Task GuildAvailable(SocketGuild guild)
    {
        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            await _syncService.GuildAvailable(guild, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue guild available for {GuildId}", guild.Id);
        }
        return Task.CompletedTask;
    }

    private Task InteractionCreated(SocketInteraction interaction)
    {
        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            if (ct.IsCancellationRequested)
            {
                return;
            }
            await _commands.ExecuteCommandAsync(ctx, _services);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue interaction created for {InteractionId}", interaction.Id);
        }
        return Task.CompletedTask;
    }

    private Task Ready()
    {
        if (Interlocked.Exchange(ref _readyInitializationQueued, 1) == 1)
        {
            return Task.CompletedTask;
        }

        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            try
            {
                _logger.LogInformation("Running one-time initialization tasks");
                ct.ThrowIfCancellationRequested();
                using (var scope = _services.CreateScope())
                {
                    await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), scope.ServiceProvider);
                }

                ct.ThrowIfCancellationRequested();
                await _commands.RegisterCommandsGloballyAsync();

                ct.ThrowIfCancellationRequested();
                if (!bool.TryParse(_config["NoSync"], out bool noSync) || !noSync)
                {
                    await _syncService.SyncOnStartup(ct);
                }

                using (var scope = _services.CreateScope())
                {
                    var versionService = scope.ServiceProvider.GetRequiredService<IVersionService>();
                    await versionService.CheckForVersionUpdate(ct);
                }
                _logger.LogInformation("Finished one-time initialization tasks");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while initializing Discord startup");
                throw;
            }
        });

        if (!enqueued)
        {
            Interlocked.Exchange(ref _readyInitializationQueued, 0);
            _logger.LogWarning("Failed to queue Discord startup initialization");
        }
        return Task.CompletedTask;
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
        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            await _syncService.ThreadCreated(thread, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue thread created for {ThreadId}", thread.Id);
        }
        return Task.CompletedTask;
    }

    private Task ThreadDeleted(Cacheable<SocketThreadChannel, ulong> thread)
    {
        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }
            var threadChannel = await thread.GetOrDownloadAsync();
            if (threadChannel == null)
            {
                _logger.LogWarning("Thread deleted: thread {ThreadId} unavailable", thread.Id);
                return;
            }
            await _syncService.ThreadDeleted(threadChannel, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue thread deleted for {ThreadId}", thread.Id);
        }
        return Task.CompletedTask;
    }
    
    private Task ThreadUpdated(Cacheable<SocketThreadChannel, ulong> oldThreadChannelCacheable, SocketThreadChannel newThreadChannel)
    {
        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }
            var oldThreadChannel = await oldThreadChannelCacheable.GetOrDownloadAsync();
            if (oldThreadChannel == null)
            {
                _logger.LogWarning("Thread updated: old thread {ThreadId} unavailable", oldThreadChannelCacheable.Id);
                return;
            }
            await _syncService.ThreadUpdated(oldThreadChannel, newThreadChannel, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue thread updated for {ThreadId}", newThreadChannel.Id);
        }
        return Task.CompletedTask;
    }
    
    private Task ThreadMemberJoined(SocketThreadUser threadUser)
    {
        if (threadUser.GuildUser.IsBot || threadUser.GuildUser.IsWebhook)
        {
            return Task.CompletedTask;
        }

        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            await _syncService.ThreadMemberJoined(threadUser, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue thread member joined for {ThreadUserId}", threadUser.Id);
        }
        return Task.CompletedTask;
    }
    
    private Task ThreadMemberLeft(SocketThreadUser threadUser)
    {
        if (threadUser.GuildUser.IsBot)
        {
            return Task.CompletedTask;
        }

        var enqueued = _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
        {
            await _syncService.ThreadMemberLeft(threadUser, ct);
        });
        if (!enqueued)
        {
            _logger.LogWarning("Failed to queue thread member left for {ThreadUserId}", threadUser.Id);
        }
        return Task.CompletedTask;
    }
}