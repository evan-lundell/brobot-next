using System.Reflection;
using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

// ReSharper disable MemberCanBeMadeStatic.Local

namespace Brobot.Services;

public class DiscordEventHandler(
    DiscordSocketClient client,
    IServiceProvider services,
    ISyncService sync,
    HotOpService hotOpService,
    IConfiguration configuration,
    ILogger<DiscordEventHandler> logger)
    : IDisposable
{
    private readonly InteractionService _commands = new(client);
    private readonly ILogger _logger = logger;

    public void Start()
    {
        client.Log += Log;
        client.Ready += Ready;
        client.InteractionCreated += InteractionCreated;
        client.GuildAvailable += GuildAvailable;
        client.GuildUnavailable += GuildUnavailable;
        client.GuildUpdated += GuildUpdated;
        client.ChannelCreated += ChannelCreated;
        client.ChannelDestroyed += ChannelDestroyed;
        client.ChannelUpdated += ChannelUpdated;
        client.MessageReceived += MessageReceived;
        client.MessageDeleted += MessageDeleted;
        client.UserVoiceStateUpdated += UserVoiceStateUpdated;
        client.PresenceUpdated += PresenceUpdated;
        client.ThreadCreated += ThreadCreated;
        client.ThreadDeleted += ThreadDeleted;
        client.ThreadUpdated += ThreadUpdated;
        client.ThreadMemberJoined += ThreadMemberJoined;
        client.ThreadMemberLeft += ThreadMemberLeft;
    }

#pragma warning disable CA1816
    public void Dispose()
#pragma warning restore CA1816
    {
        client.Log -= Log;
        client.Ready -= Ready;
        client.InteractionCreated -= InteractionCreated;
        client.GuildAvailable -= GuildAvailable;
        client.GuildUnavailable -= GuildUnavailable;
        client.ChannelCreated -= ChannelCreated;
        client.ChannelDestroyed -= ChannelDestroyed;
        client.ChannelUpdated -= ChannelUpdated;
        client.MessageReceived -= MessageReceived;
        client.MessageDeleted -= MessageDeleted;
        client.PresenceUpdated -= PresenceUpdated;
        client.ThreadCreated -= ThreadCreated;
        client.ThreadDeleted -= ThreadDeleted;
        client.ThreadUpdated -= ThreadUpdated;
        client.ThreadMemberJoined -= ThreadMemberJoined;
        client.ThreadMemberLeft -= ThreadMemberLeft;
    }

    private Task PresenceUpdated(SocketUser socketUser, SocketPresence formerPresence, SocketPresence currentPresence)
    {
        return sync.PresenceUpdated(socketUser, formerPresence, currentPresence);
    }

    private Task UserVoiceStateUpdated(
        SocketUser socketUser,
        SocketVoiceState previousVoiceState,
        SocketVoiceState currentVoiceState)
    {
        return hotOpService.UserVoiceStateUpdated(socketUser, previousVoiceState, currentVoiceState);
    }

    private Task MessageReceived(SocketMessage socketMessage)
    {
        return Task.Run(async () =>
        {
            if (socketMessage.Author.IsBot)
            {
                return;
            }
            
            using var scope = services.CreateScope();
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

            var fixTwitterLinks = bool.Parse(configuration["FixTwitterLinks"] ?? "false");
            if (fixTwitterLinks)
            {
                if (socketMessage.Content.Contains("https://twitter.com"))
                {
                    var newMessage = socketMessage.Content.Replace("https://twitter.com", "https://vxtwitter.com");
                    await socketMessage.Channel.SendMessageAsync(newMessage);
                }

                if (socketMessage.Content.Contains("https://x.com"))
                {
                    var newMessage = socketMessage.Content.Replace("https://x.com", "https://vxtwitter.com");
                    await socketMessage.Channel.SendMessageAsync(newMessage);
                }
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
        return sync.GuildUpdated(previous, current);
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

        return sync.ChannelUpdated(previousTextChannel, currentTextChannel);
    }

    private Task ChannelDestroyed(SocketChannel channel)
    {
        if (channel is not SocketTextChannel textChannel || channel is SocketVoiceChannel)
        {
            return Task.CompletedTask;
        }
        return sync.ChannelDestroyed(textChannel);
    }

    private Task ChannelCreated(SocketChannel channel)
    {
        if (channel is not SocketTextChannel textChannel || channel is SocketVoiceChannel)
        {
            return Task.CompletedTask;
        }
        return sync.ChannelCreated(textChannel);
    }

    private Task GuildUnavailable(SocketGuild guild)
    {
        return sync.GuildUnavailable(guild);
    }

    private Task GuildAvailable(SocketGuild guild)
    {
        return sync.GuildAvailable(guild);
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        var ctx = new SocketInteractionContext(client, interaction);
        await _commands.ExecuteCommandAsync(ctx, services);
    }

    private async Task Ready()
    {

        using (var scope = services.CreateScope())
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), scope.ServiceProvider);
        }

        if (configuration["DOTNET_ENVIRONMENT"] == "Development")
        {
            _ = _commands.RegisterCommandsToGuildAsync(421404457599762433);
        }
        else
        {
            _ = _commands.RegisterCommandsGloballyAsync();
        }

        if (!bool.TryParse(configuration["NoSync"], out bool noSync) || !noSync)
        {
            _ = sync.SyncOnStartup();
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
        using var scope = services.CreateScope();
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
        using var scope = services.CreateScope();
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
        
        using var scope = services.CreateScope();
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
        
        using var scope = services.CreateScope();
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
        
        using var scope = services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var existingChannelModel = await uow.Channels.GetByIdWithChannelUsers(threadUser.Thread.Id);
        var existingDiscordUser = await uow.Users.GetById(threadUser.Id);

        if (existingChannelModel == null || existingDiscordUser == null)
        {
            return;
        }

        var channelUserModel =
            existingChannelModel.ChannelUsers.FirstOrDefault(cu => cu.UserId == existingDiscordUser.Id);
        if (channelUserModel == null)
        {
            return;
        }
        existingChannelModel.ChannelUsers.Remove(channelUserModel);
        await uow.CompleteAsync();
    }
}