using Brobot.Models;
using Brobot.Configuration;
using Brobot.Repositories;
using Brobot.Shared;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Brobot.Services;

public class SyncService(IServiceScopeFactory serviceScopeFactory, IDiscordClient discordClient, ILogger<SyncService> logger, IOptions<GeneralOptions> generalOptions) : ISyncService
{
    public async Task ChannelCreated(IGuildChannel channel)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var guild = await uow.Guilds.GetById(channel.GuildId);
        if (guild == null)
        {
            return;
        }
        var userModels = (await uow.Users.GetAllWithGuildsAndChannels())
            .Where(u => u.Archived == false)
            .ToDictionary(u => u.Id);
        var newChannel = new ChannelModel
        {
            Id = channel.Id,
            Name = channel.Name,
            Guild = guild,
            GuildId = guild.Id
        };
        
        await foreach (var user in channel.GetUsersAsync().Flatten())
        {
            newChannel.ChannelUsers.Add(new ChannelUserModel
            {
                Channel = newChannel,
                ChannelId = channel.Id,
                User = userModels[user.Id],
                UserId = user.Id
            });
        }

        await uow.Channels.Add(newChannel);
        await uow.CompleteAsync();
    }

    public async Task ChannelDestroyed(IGuildChannel channel)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var channelToBeDeleted = await uow.Channels.GetById(channel.Id);
        if (channelToBeDeleted == null)
        {
            return;
        }

        uow.Channels.Remove(channelToBeDeleted);
        await uow.CompleteAsync();
    }

    public async Task ChannelUpdated(IGuild guild, ISocketMessageChannel previous, ISocketMessageChannel current)
    {
        if (previous.Name == current.Name)
        {
            return;
        }
        
        var userName = "";
        var auditLogs = await guild.GetAuditLogsAsync(limit: 1, actionType: ActionType.ChannelUpdated);
        var auditLog = auditLogs.FirstOrDefault();
        if (auditLog != null)
        {
            userName = auditLog.User.Username;
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            await current.SendMessageAsync($"{userName} changed the channel name from '{previous.Name}' to '{current.Name}'");
        }
        else
        {
            await current.SendMessageAsync($"Channel name changed from '{previous.Name}' to '{current.Name}'");
        }

        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var channelModel = await uow.Channels.GetById(current.Id);
        if (channelModel == null)
        {
            return;
        }
        channelModel.Name = current.Name;
        await uow.CompleteAsync();
    }

    public async Task GuildAvailable(IGuild guild)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var guildModel = await uow.Guilds.GetById(guild.Id);
        if (guildModel != null)
        {
            return;
        }

        var userLookup = (await uow.Users.GetAllWithGuildsAndChannels())
            .Where(u => u.Archived == false)
            .ToDictionary(u => u.Id);
        var newGuild = new GuildModel
        {
            Id = guild.Id,
            Name = guild.Name
        };
        await uow.Guilds.Add(newGuild);

        var channels = await guild.GetChannelsAsync();

        HashSet<ulong> usersAddedToGuild = [];
        foreach (var channel in channels.Where(c => c.ChannelType == ChannelType.Text))
        {
            var newChannel = new ChannelModel
            {
                Id = channel.Id,
                Name = channel.Name,
                Guild = newGuild,
                GuildId = guild.Id
            };
            await uow.Channels.Add(newChannel);

            await foreach (var user in channel.GetUsersAsync().Flatten().Where(u => !u.IsBot))
            {
                if (!userLookup.ContainsKey(user.Id))
                {
                    var newUser = new UserModel
                    {
                        Id = user.Id,
                        Username = user.Username
                    };
                    userLookup.Add(user.Id, newUser);
                }

                if (!usersAddedToGuild.Contains(user.Id))
                {
                    newGuild.GuildUsers.Add(new GuildUserModel
                    {
                        Guild = newGuild,
                        GuildId = guild.Id,
                        User = userLookup[user.Id],
                        UserId = user.Id
                    });
                    usersAddedToGuild.Add(user.Id);
                }

                newChannel.ChannelUsers.Add(new ChannelUserModel
                {
                    Channel = newChannel,
                    ChannelId = channel.Id,
                    User = userLookup[user.Id],
                    UserId = user.Id
                });
            }
        }

        await uow.CompleteAsync();
    }

    public async Task GuildUnavailable(IGuild guild)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var guildToBeDeleted = await uow.Guilds.GetById(guild.Id);
        if (guildToBeDeleted == null)
        {
            return;
        }
        uow.Guilds.Remove(guildToBeDeleted);
        await uow.CompleteAsync();
    }

    public async Task GuildUpdated(IGuild previousGuild, IGuild currentGuild)
    {
        if (previousGuild.Name == currentGuild.Name)
        {
            return;
        }

        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var guildModel = await uow.Guilds.GetById(currentGuild.Id);
        if (guildModel == null)
        {
            return;
        }
        guildModel.Name = currentGuild.Name;
        await uow.CompleteAsync();
    }

    public async Task SyncOnStartup()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var guildModels = (await uow.Guilds.Find(g => g.Archived == false)).ToDictionary(g => g.Id);
        var channelModels = (await uow.Channels.Find(c => c.Archived == false)).ToDictionary(c => c.Id);
        var userModels = (await uow.Users.GetAllWithGuildsAndChannels()).Where(u => u.Archived == false).ToDictionary(u => u.Id);

        var guildIds = new HashSet<ulong>();
        var channelIds = new HashSet<ulong>();
        var userIds = new HashSet<ulong>();

        // Create new entities or update existing entities
        var guilds = await discordClient.GetGuildsAsync();
        foreach (var guild in guilds)
        {
            guildIds.Add(guild.Id);
            if (!guildModels.TryGetValue(guild.Id, out var guildModel))
            {
                var newGuild = new GuildModel
                {
                    Id = guild.Id,
                    Name = guild.Name
                };
                await uow.Guilds.Add(newGuild);
                guildModels.Add(guild.Id, newGuild);
            }
            else if (guildModel.Name != guild.Name)
            {
                guildModel.Name = guild.Name;
            }

            var channels = await guild.GetChannelsAsync();
            foreach (var channel in channels)
            {
                if (channel.ChannelType != ChannelType.Text)
                {
                    continue;
                }
                channelIds.Add(channel.Id);
                if (!channelModels.TryGetValue(channel.Id, out var channelModel))
                {
                    var newChannel = new ChannelModel
                    {
                        Id = channel.Id,
                        Name = channel.Name,
                        Guild = guildModels[guild.Id],
                        GuildId = guild.Id
                    };
                    channelModels.Add(channel.Id, newChannel);
                    await uow.Channels.Add(newChannel);
                }
                else if (channelModel.Name != channel.Name)
                {
                    channelModel.Name = channel.Name;
                }
                
                await foreach (var user in channel.GetUsersAsync().Flatten().Where(u => !u.IsBot))
                {
                    userIds.Add(user.Id);
                    if (!userModels.TryGetValue(user.Id, out var userModel))
                    {
                        var newUser = new UserModel
                        {
                            Id = user.Id,
                            Username = user.Username
                        };
                        userModels.Add(user.Id, newUser);
                        await uow.Users.Add(newUser);
                    }
                    else if (userModel.Username != user.Username)
                    {
                        userModel.Username = user.Username;
                    }

                    if (userModels[user.Id].ChannelUsers.All(cu => cu.ChannelId != channel.Id))
                    {
                        userModels[user.Id].ChannelUsers.Add(new ChannelUserModel
                        {
                            User = userModels[user.Id],
                            UserId = user.Id,
                            Channel = channelModels[channel.Id],
                            ChannelId = channel.Id
                        });
                    }

                    if (userModels[user.Id].GuildUsers.All(gu => gu.GuildId != guild.Id))
                    {
                        userModels[user.Id].GuildUsers.Add(new GuildUserModel
                        {
                            User = userModels[user.Id],
                            UserId = user.Id,
                            Guild = guildModels[guild.Id],
                            GuildId = guild.Id
                        });
                    }
                }
            }
        }

        // Remove old entities
        foreach (var guild in guildModels.Values.Where(g => !guildIds.Contains(g.Id)))
        {
            uow.Guilds.Remove(guild);
        }

        foreach (var channel in channelModels.Values.Where(c => !channelIds.Contains(c.Id)))
        {
            uow.Channels.Remove(channel);
        }

        foreach (var user in userModels.Values.Where(u => !userIds.Contains(u.Id)))
        {
            uow.Users.Remove(user);
        }

        await uow.CompleteAsync();
    }

    public async Task PresenceUpdated(IUser socketUser, IPresence formerSocketPresence, IPresence currentSocketPresence)
    {
        if (currentSocketPresence.Status == UserStatus.Online || socketUser.IsBot)
        {
            return;
        }

        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var user = await uow.Users.GetById(socketUser.Id);
        if (user == null)
        {
            return;
        }

        user.LastOnline = DateTime.UtcNow;
        await uow.CompleteAsync();
    }

    public async Task UserVoiceStateUpdated(IUser user, IVoiceState previousVoiceState, IVoiceState currentVoiceState)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var hotOpService = scope.ServiceProvider.GetRequiredService<IHotOpService>();
        if (previousVoiceState.VoiceChannel == null && currentVoiceState.VoiceChannel != null)
        {
            var connectedUsers = await currentVoiceState.VoiceChannel.GetUsersAsync().FlattenAsync();
            await hotOpService.UpdateHotOps(user.Id, UserVoiceStateAction.Connected, connectedUsers.Select(u => u.Id).ToList());
        }

        if (currentVoiceState.VoiceChannel == null && previousVoiceState.VoiceChannel != null)
        {
            var connectedUsers = await previousVoiceState.VoiceChannel.GetUsersAsync().FlattenAsync();
            await hotOpService.UpdateHotOps(user.Id, UserVoiceStateAction.Disconnected,  connectedUsers.Select(u => u.Id).ToList());
        }
    }

    public async Task MessageReceived(IMessage message)
    {
        if (message.Author.IsBot)
        {
            return;
        }
            
        using var scope = serviceScopeFactory.CreateScope();
        var messageCountService = scope.ServiceProvider.GetRequiredService<IMessageCountService>();
        await messageCountService.AddToDailyCount(message.Author.Id, message.Channel.Id);
            
        switch (message.Content.ToLower())
        {
            case "good bot":
                await message.Channel.SendMessageAsync("Thanks! :robot:");
                break;
            case "bad bot":
                await message.Channel.SendMessageAsync(":middle_finger:");
                break;
        }

        var fixTwitterLinks = generalOptions.Value.FixTwitterLinks;
        if (fixTwitterLinks)
        {
            if (message.Content.Contains("https://twitter.com"))
            {
                var newMessage = message.Content.Replace("https://twitter.com", "https://vxtwitter.com");
                await message.Channel.SendMessageAsync(newMessage);
            }

            if (message.Content.Contains("https://x.com"))
            {
                var newMessage = message.Content.Replace("https://x.com", "https://vxtwitter.com");
                await message.Channel.SendMessageAsync(newMessage);
            }
        }
    }

    public async Task MessageDeleted(IMessage message,
        IMessageChannel channel, IGuild guild)
    {
        try
        {
            var now = DateTimeOffset.UtcNow;
            if (channel.ChannelType != ChannelType.Text)
            {
                return;
            }
                
            var auditLog = (await guild.GetAuditLogsAsync(limit: 1, actionType: ActionType.MessageDeleted))
                .FirstOrDefault();
            if (auditLog == null)
            {
                await channel.SendMessageAsync($"I saw that {message.Author.Username} :spy:");
                return;
            }

            var username = now - auditLog.CreatedAt < TimeSpan.FromSeconds(5)
                ? auditLog.User.Username
                : message.Author.Username;

            await channel.SendMessageAsync($"I saw that {username} :spy:");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Message deleted failed");
        }
    }

    public async Task ThreadCreated(IThreadChannel thread)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var guild = await uow.Guilds.GetById(thread.Guild.Id);
        if (guild == null)
        {
            logger.LogWarning("Thread creation failed. Guild {GuildId} does not exist", thread.Guild.Id);
            return;
        }
        
        var existingThread = await uow.Channels.GetById(thread.Id);
        if (existingThread is { Archived: false })
        {
            logger.LogWarning("Thread creation failed. Thread with ID {ThreadId} already exists", thread.Id);
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
            
            var discordUser = await uow.Users.GetById(thread.OwnerId);
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

    public async Task ThreadDeleted(IThreadChannel thread)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var existingThread = await uow.Channels.GetById(thread.Id);
        if (existingThread == null)
        {
            return;
        }
        uow.Channels.Remove(existingThread);
        await uow.CompleteAsync();
    }

    public async Task ThreadMemberJoined(IThreadUser user)
    {
        if (user.GuildUser.IsBot || user.GuildUser.IsWebhook)
        {
            return;
        }
        
        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var existingChannelModel = await uow.Channels.GetByIdWithChannelUsers(user.Thread.Id);
        var existingDiscordUser = await uow.Users.GetById(user.GuildUser.Id);

        if (existingChannelModel == null || existingDiscordUser == null)
        {
            return;
        }

        if (existingChannelModel.ChannelUsers.All(cu => cu.UserId != user.GuildUser.Id))
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

    public async Task ThreadUpdated(IThreadChannel oldThread, IThreadChannel newThread)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var existingChannelModel = await uow.Channels.GetById(oldThread.Id);
        if (existingChannelModel == null || (existingChannelModel.Name == newThread.Name && existingChannelModel.Archived == newThread.IsArchived))
        {
            return;
        }

        existingChannelModel.Name = newThread.Name;
        existingChannelModel.Archived = newThread.IsArchived;
        await uow.CompleteAsync();
    }

    public async Task ThreadMemberLeft(IThreadUser threadUser)
    {
        if (threadUser.GuildUser.IsBot)
        {
            return;
        }
        
        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var existingChannelModel = await uow.Channels.GetByIdWithChannelUsers(threadUser.Thread.Id);
        var existingDiscordUser = await uow.Users.GetById(threadUser.GuildUser.Id);

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