using System.Reflection;
using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TimeZoneConverter;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;
// ReSharper disable MemberCanBeMadeStatic.Local

namespace Brobot.Services;

public class DiscordEventHandler : IDisposable, IAsyncDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _services;
    private readonly ISyncService _sync;
    private readonly InteractionService _commands;
    private readonly HotOpService _hotOpService;
    private readonly LavaNode _node;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public DiscordEventHandler(
        DiscordSocketClient client,
        IServiceProvider services,
        ISyncService sync,
        HotOpService hotOpService,
        LavaNode node,
        IConfiguration configuration,
        ILogger<DiscordEventHandler> logger)
    {
        _client = client;
        _services = services;
        _sync = sync;
        _commands = new InteractionService(client);
        _hotOpService = hotOpService;
        _node = node;
        _configuration = configuration;
        _logger = logger;
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
        _node.OnTrackEnd += OnTrackEnd;
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
        _node.OnTrackEnd -= OnTrackEnd;
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
            switch (socketMessage.Content.ToLower())
            {
                case "good bot":
                    await socketMessage.Channel.SendMessageAsync("Thanks! :robot:");
                    break;
                case "bad bot":
                    await socketMessage.Channel.SendMessageAsync(":middle_finger:");
                    break;
            }

            if (socketMessage.Author.IsBot)
            {
                return;
            }
            
            using var scope = _services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var user = await uow.Users.GetById(socketMessage.Author.Id);
            if (string.IsNullOrWhiteSpace(user?.Timezone))
            {
                return;
            }

            var timezone = TZConvert.GetTimeZoneInfo(user.Timezone);
            var userTimeNow = DateTime.UtcNow + timezone.GetUtcOffset(DateTime.UtcNow);
            var dailyCount = await uow.DailyMessageCounts.GetByUserAndDay(user.Id, DateOnly.FromDateTime(userTimeNow));
            if (dailyCount == null)
            {
                await uow.DailyMessageCounts.Add(new DailyMessageCountModel
                {
                    User = user,
                    UserId = user.Id,
                    CountDate = DateOnly.FromDateTime(userTimeNow),
                    MessageCount = 1
                });
            }
            else
            {
                dailyCount.MessageCount++;
            }

            await uow.CompleteAsync();
        });
    }

    private Task MessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        return Task.Run(async () =>
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var channel = await cachedChannel.GetOrDownloadAsync();
                var message = await cachedMessage.GetOrDownloadAsync() ??
                              await channel.GetMessageAsync(cachedMessage.Id);
                if (channel is not SocketTextChannel textChannel || channel is SocketVoiceChannel)
                {
                    return;
                }
                
                var auditLog = (await textChannel.Guild
                    .GetAuditLogsAsync(limit: 1, actionType: ActionType.MessageDeleted).FlattenAsync()).FirstOrDefault();
                if (auditLog == null)
                {
                    await textChannel.SendMessageAsync($"I saw that {message.Author.Username} :spy:");
                    return;
                }

                var username = now - auditLog.CreatedAt < TimeSpan.FromSeconds(5)
                    ? auditLog.User.Username
                    : message.Author.Username;

                await textChannel.SendMessageAsync($"I saw that {username} :spy:");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message deleted failed");
            }
        });
    }

    private Task GuildUpdated(SocketGuild previous, SocketGuild current)
    {
        return _sync.GuildUpdated(previous, current);
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

        return _sync.ChannelUpdated(previousTextChannel, currentTextChannel);
    }

    private Task ChannelDestroyed(SocketChannel channel)
    {
        if (channel is not SocketTextChannel textChannel || channel is SocketVoiceChannel)
        {
            return Task.CompletedTask;
        }
        return _sync.ChannelDestroyed(textChannel);
    }

    private Task ChannelCreated(SocketChannel channel)
    {
        if (channel is not SocketTextChannel textChannel || channel is SocketVoiceChannel)
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

        if (_configuration["DOTNET_ENVIRONMENT"] == "Development")
        {
            _ = _commands.RegisterCommandsToGuildAsync(421404457599762433);
        }
        else
        {
            _ = _commands.RegisterCommandsGloballyAsync();
        }

        if (!bool.TryParse(_configuration["NoSync"], out bool noSync) || !noSync)
        {
            _ = _sync.SyncOnStartup();
        }

        try
        {
            await _node.ConnectAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
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

    private async Task OnTrackEnd(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        _logger.LogInformation("Track ended");

        var player = arg.Player;
        if (player.Vueue.Count == 0)
        {
            _logger.LogInformation("Queue empty");
            return;
        }
        
        if (arg.Reason != TrackEndReason.Finished)
        {
            _logger.LogInformation("Not playing next, track end reason {Reason}", arg.Reason.ToString());
            return;
        }

        
        if (!player.Vueue.TryDequeue(out LavaTrack nextTrack))
        {
            _logger.LogInformation("Not playing next, unable to dequeue next track");
            return;
        }

        _logger.LogInformation("Queuing next track");
        await player.PlayAsync(nextTrack);
    }

#pragma warning disable CA1816
    public async ValueTask DisposeAsync()
#pragma warning restore CA1816
    {
        foreach (var player in _node.Players)
        {
            if (player.VoiceChannel != null)
            {
                await _node.LeaveAsync(player.VoiceChannel);
            }
        }
        await _node.DisconnectAsync();
        Dispose();
    }
}