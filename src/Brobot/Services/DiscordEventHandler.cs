using System.Reflection;
using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
// ReSharper disable MemberCanBeMadeStatic.Local

namespace Brobot.Services;

public class DiscordEventHandler : IDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _services;
    private readonly ISyncService _sync;
    private readonly InteractionService _commands;
    private readonly HotOpService _hotOpService;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public DiscordEventHandler(
        DiscordSocketClient client,
        IServiceProvider services,
        ISyncService sync,
        HotOpService hotOpService,
        IConfiguration configuration,
        ILogger<DiscordEventHandler> logger)
    {
        _client = client;
        _services = services;
        _sync = sync;
        _commands = new InteractionService(client);
        _hotOpService = hotOpService;
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
            if (socketMessage.Author.IsBot)
            {
                return;
            }
            
            using var scope = _services.CreateScope();
            var messageCountService = scope.ServiceProvider.GetRequiredService<MessageCountService>();
            await messageCountService.AddToDailyCount(socketMessage.Author.Id, socketMessage.Channel.Id);
            
            switch (socketMessage.Content.ToLower())
            {
                case "good bot":
                    await socketMessage.Channel.SendMessageAsync("Thanks! :robot:");
                    break;
                case "bad bot":
                    await socketMessage.Channel.SendMessageAsync(":middle_finger:");
                    break;
            }

            if (socketMessage.Content.Contains("twitter.com"))
            {
                var newMessage = socketMessage.Content.Replace("twitter.com", "vxtwitter.com");
                await socketMessage.Channel.SendMessageAsync(newMessage);
            }
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

    private async Task ThreadCreated(SocketThreadChannel thread)
    {
        using var scope = _services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var guild = await uow.Guilds.GetById(thread.Guild.Id);
        if (guild == null)
        {
            _logger.LogWarning("Thread creation failed. Guild {GuildId} does not exist", thread.Guild.Id);
            return;
        }
        
        var existingThread = await uow.Channels.GetById(thread.Id);
        if (existingThread is { Archived: false })
        {
            _logger.LogWarning("Thread creation failed. Thread with ID {ThreadId} already exists", thread.Id);
            return;
        }
        
        if (existingThread != null)
        {
            existingThread.Archived = false;
        }
        else
        {
            var newThread = new ChannelModel
            {
                Id = thread.Id,
                Name = thread.Name,
                Guild = guild,
                GuildId = thread.Guild.Id
            };
            await uow.Channels.Add(newThread);

            var discordUser = await uow.Users.GetById(thread.Owner.Id);
            if (discordUser != null)
            {
                newThread.ChannelUsers.Add(new ChannelUserModel
                {
                    User = discordUser,
                    UserId = discordUser.Id,
                    Channel = newThread,
                    ChannelId = newThread.Id
                });
            }
        }

        await uow.CompleteAsync();
    }

    private async Task ThreadDeleted(Cacheable<SocketThreadChannel, ulong> thread)
    {
        using var scope = _services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var existingThread = await uow.Channels.GetById(thread.Id);
        if (existingThread == null)
        {
            return;
        }
        uow.Channels.Remove(existingThread);
        await uow.CompleteAsync();
    }
    
    private async Task ThreadUpdated(Cacheable<SocketThreadChannel, ulong> oldThreadChannelCacheable, SocketThreadChannel newThreadChannel)
    {
        var oldThreadChannel = await oldThreadChannelCacheable.GetOrDownloadAsync();
        if (oldThreadChannel == null)
        {
            return;
        }
        
        using var scope = _services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var existingChannelModel = await uow.Channels.GetById(oldThreadChannel.Id);
        if (existingChannelModel == null || (existingChannelModel.Name == newThreadChannel.Name && existingChannelModel.Archived == newThreadChannel.IsArchived))
        {
            return;
        }

        existingChannelModel.Name = newThreadChannel.Name;
        existingChannelModel.Archived = newThreadChannel.IsArchived;
        await uow.CompleteAsync();
    }
    
    private async Task ThreadMemberJoined(SocketThreadUser threadUser)
    {
        if (threadUser.IsBot)
        {
            return;
        }
        
        using var scope = _services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var existingChannelModel = await uow.Channels.GetByIdWithChannelUsers(threadUser.Thread.Id);
        var existingDiscordUser = await uow.Users.GetById(threadUser.Id);

        if (existingChannelModel == null || existingDiscordUser == null)
        {
            return;
        }

        if (existingChannelModel.ChannelUsers.All(cu => cu.UserId != threadUser.Id))
        {
            existingChannelModel.ChannelUsers.Add(new ChannelUserModel
            {
                Channel = existingChannelModel,
                ChannelId = existingChannelModel.Id,
                User = existingDiscordUser,
                UserId = existingDiscordUser.Id
            });
            await uow.CompleteAsync();
        }
    }
    
    private async Task ThreadMemberLeft(SocketThreadUser threadUser)
    {
        if (threadUser.IsBot)
        {
            return;
        }
        
        using var scope = _services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var existingChannelModel = await uow.Channels.GetByIdWithChannelUsers(threadUser.Thread.Id);
        var existingDiscordUser = await uow.Users.GetById(threadUser.Id);

        if (existingChannelModel == null || existingDiscordUser == null)
        {
            return;
        }

        var channelUserModel =
            existingChannelModel.ChannelUsers.FirstOrDefault((cu) => cu.UserId == existingDiscordUser.Id);
        if (channelUserModel == null)
        {
            return;
        }
        existingChannelModel.ChannelUsers.Remove(channelUserModel);
        await uow.CompleteAsync();
    }
}