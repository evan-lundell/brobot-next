using Brobot.Models;
using Brobot.Configuration;
using Brobot.Repositories;
using Brobot.Shared;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Brobot.Services;

public class SyncService(
    IServiceScopeFactory serviceScopeFactory,
    IDiscordClient discordClient,
    ILogger<SyncService> logger,
    IOptions<GeneralOptions> generalOptions) : ISyncService
{
    public async Task ChannelCreated(IGuildChannel channel)
    {
        try
        {
            logger.LogInformation("New channel created with ChannelId {ChannelId}", channel.Id);
            using var scope = serviceScopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var guild = await uow.Guilds.GetById(channel.GuildId);
            if (guild == null)
            {
                logger.LogWarning("No guild found with Id of {GuildId}", channel.GuildId);
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
            logger.LogInformation("Channel {ChannelId} added to database.", channel.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing channel created for ChannelId {ChannelId}", channel.Id);
        }
    }

    public async Task ChannelDestroyed(IGuildChannel channel)
    {
        try
        {
            logger.LogInformation("Channel {ChannelId} was destroyed.", channel.Id);
            using var scope = serviceScopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var channelToBeDeleted = await uow.Channels.GetById(channel.Id);
            if (channelToBeDeleted == null)
            {
                logger.LogWarning("Channel {ChannelId} could not be found.", channel.Id);
                return;
            }

            uow.Channels.Remove(channelToBeDeleted);
            await uow.CompleteAsync();
            logger.LogInformation("Channel {ChannelId} removed from database.", channel.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing channel destroyed for ChannelId {ChannelId}", channel.Id);
        }
    }

    public async Task ChannelUpdated(IGuild guild, ISocketMessageChannel previous, ISocketMessageChannel current)
    {
        try
        {
            if (previous.Name == current.Name)
            {
                return;
            }

            logger.LogInformation("Channel {ChannelId} name updated from {PreviousName} to {CurrentName}.",
                previous.Name, current.Name, previous.Name);
            var userName = "";
            var auditLogs = await guild.GetAuditLogsAsync(limit: 1, actionType: ActionType.ChannelUpdated);
            var auditLog = auditLogs.FirstOrDefault();
            if (auditLog != null)
            {
                userName = auditLog.User.Username;
            }

            logger.LogInformation("Sending name update message");
            if (!string.IsNullOrWhiteSpace(userName))
            {
                await current.SendMessageAsync(
                    $"{userName} changed the channel name from '{previous.Name}' to '{current.Name}'");
            }
            else
            {
                await current.SendMessageAsync($"Channel name changed from '{previous.Name}' to '{current.Name}'");
            }

            logger.LogInformation("Finished sending name update message");

            logger.LogInformation("Updating database");
            using var scope = serviceScopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var channelModel = await uow.Channels.GetById(current.Id);
            if (channelModel == null)
            {
                logger.LogWarning("Channel {ChannelId} could not be found in database.", current.Id);
                return;
            }

            channelModel.Name = current.Name;
            await uow.CompleteAsync();
            logger.LogInformation("Finished updating name in database");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing channel updated for ChannelId {ChannelId}", current.Id);
        }
    }

    public async Task GuildAvailable(IGuild guild)
    {
        try
        {
            logger.LogInformation("Guild {GuildId} is now available", guild.Id);
            using var scope = serviceScopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var guildModel = await uow.Guilds.GetById(guild.Id);
            if (guildModel != null)
            {
                logger.LogInformation("Guild {GuildId} is already in the database", guild.Id);
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
            logger.LogInformation("Guild {GuildId} added to database", guild.Id);

            var channels = await guild.GetChannelsAsync();
            HashSet<ulong> usersAddedToGuild = [];
            foreach (var channel in channels.Where(c => c.ChannelType == ChannelType.Text))
            {
                logger.LogInformation("Adding channel {ChannelId} to the database", channel.Id);
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
                    logger.LogInformation("Processing user {UserId}", user.Id);
                    if (!userLookup.ContainsKey(user.Id))
                    {
                        logger.LogInformation("User {UserId} not found, creating a new one", user.Id);
                        var newUser = new UserModel
                        {
                            Id = user.Id,
                            Username = user.Username
                        };
                        userLookup.Add(user.Id, newUser);
                    }

                    if (!usersAddedToGuild.Contains(user.Id))
                    {
                        logger.LogInformation("Associating user {UserId} to guild {GuildId}", user.Id, guild.Id);
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

                    logger.LogInformation("Finished processing user {UserId}", user.Id);
                }

                logger.LogInformation("Finished adding channel {ChannelId} to the database", channel.Id);
            }

            await uow.CompleteAsync();
            logger.LogInformation("Finished processing guild {GuildId}", guild.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing guild {GuildId}", guild.Id);
        }
    }

    public async Task GuildUnavailable(IGuild guild)
    {
        try
        {
            logger.LogInformation("Guild {GuildId} is now unavailable", guild.Id);
            using var scope = serviceScopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var guildToBeDeleted = await uow.Guilds.GetById(guild.Id);
            if (guildToBeDeleted == null)
            {
                return;
            }

            uow.Guilds.Remove(guildToBeDeleted);
            await uow.CompleteAsync();
            logger.LogInformation("Finished deleting guild {GuildId}", guild.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing guild unavailable for GuildId {GuildId}", guild.Id);
        }
    }

        public async Task GuildUpdated(IGuild previousGuild, IGuild currentGuild)
        {
            try
            {
                if (previousGuild.Name == currentGuild.Name)
                {
                    return;
                }

                logger.LogInformation("Updating guild name from {PreviousGuildName} to {CurrentGuildName}",
                    previousGuild.Name, currentGuild.Name);
                using var scope = serviceScopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var guildModel = await uow.Guilds.GetById(currentGuild.Id);
                if (guildModel == null)
                {
                    return;
                }

                guildModel.Name = currentGuild.Name;
                await uow.CompleteAsync();
                logger.LogInformation("Finished updating guild {GuildId}", currentGuild.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing guild unavailable for GuildId {GuildId}", previousGuild.Id);
            }
        }

    public async Task SyncOnStartup()
    {
        try
        {
            logger.LogInformation("Starting up sync service");
            using var scope = serviceScopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var guildModels = (await uow.Guilds.Find(g => g.Archived == false)).ToDictionary(g => g.Id);
            var channelModels = (await uow.Channels.Find(c => c.Archived == false)).ToDictionary(c => c.Id);
            var userModels = (await uow.Users.GetAllWithGuildsAndChannels()).Where(u => u.Archived == false)
                .ToDictionary(u => u.Id);

            var guildIds = new HashSet<ulong>();
            var channelIds = new HashSet<ulong>();
            var userIds = new HashSet<ulong>();

            // Create new entities or update existing entities
            var guilds = await discordClient.GetGuildsAsync();
            foreach (var guild in guilds)
            {
                logger.LogInformation("Processing guild {GuildId}", guild.Id);
                guildIds.Add(guild.Id);
                if (!guildModels.TryGetValue(guild.Id, out var guildModel))
                {
                    logger.LogInformation("Guild {GuildId} does not exist, adding to database", guild.Id);
                    var newGuild = new GuildModel
                    {
                        Id = guild.Id,
                        Name = guild.Name
                    };
                    await uow.Guilds.Add(newGuild);
                    guildModels.Add(guild.Id, newGuild);
                    logger.LogInformation("Added guild {GuildId} to the database", guild.Id);
                }
                else if (guildModel.Name != guild.Name)
                {
                    logger.LogInformation("Updating guild {GuildId} name from {PreviousName} to {CurrentName}",
                        guild.Id, guildModel.Name, guild.Name);
                    guildModel.Name = guild.Name;
                }

                var channels = await guild.GetChannelsAsync();
                foreach (var channel in channels)
                {
                    if (channel.ChannelType != ChannelType.Text)
                    {
                        continue;
                    }

                    logger.LogInformation("Processing channel {ChannelId}", channel.Id);
                    channelIds.Add(channel.Id);
                    if (!channelModels.TryGetValue(channel.Id, out var channelModel))
                    {
                        logger.LogInformation("Channel {ChannelId} does not exist, adding to database", channel.Id);
                        var newChannel = new ChannelModel
                        {
                            Id = channel.Id,
                            Name = channel.Name,
                            Guild = guildModels[guild.Id],
                            GuildId = guild.Id
                        };
                        channelModels.Add(channel.Id, newChannel);
                        await uow.Channels.Add(newChannel);
                        logger.LogInformation("Added channel {ChannelId} to the database", channel.Id);
                    }
                    else if (channelModel.Name != channel.Name)
                    {
                        channelModel.Name = channel.Name;
                        logger.LogInformation("Updating channel {ChannelId} name from {PreviousName} to {CurrentName}",
                            channel.Id, channelModel.Name, channel.Name);
                    }

                    await foreach (var user in channel.GetUsersAsync().Flatten().Where(u => !u.IsBot))
                    {
                        logger.LogInformation("Processing user {UserId}", user.Id);
                        userIds.Add(user.Id);
                        if (!userModels.TryGetValue(user.Id, out var userModel))
                        {
                            logger.LogInformation("User {UserId} does not exist, adding to database", user.Id);
                            var newUser = new UserModel
                            {
                                Id = user.Id,
                                Username = user.Username
                            };
                            userModels.Add(user.Id, newUser);
                            await uow.Users.Add(newUser);
                            logger.LogInformation("Added user {UserId} to the database", user.Id);
                        }
                        else if (userModel.Username != user.Username)
                        {
                            userModel.Username = user.Username;
                            logger.LogInformation(
                                "User {UserId} username updated from {PreviousUsername} to {CurrentUsername}", user.Id,
                                userModel.Username, userModel.Username);
                        }

                        if (userModels[user.Id].ChannelUsers.All(cu => cu.ChannelId != channel.Id))
                        {
                            logger.LogInformation(
                                "User {UserId} is not associated to channel {ChannelId}. Creating association", user.Id,
                                channel.Id);
                            userModels[user.Id].ChannelUsers.Add(new ChannelUserModel
                            {
                                User = userModels[user.Id],
                                UserId = user.Id,
                                Channel = channelModels[channel.Id],
                                ChannelId = channel.Id
                            });
                            logger.LogInformation("User {UserId} is associated to channel {ChannelId}", user.Id,
                                channel.Id);
                        }

                        if (userModels[user.Id].GuildUsers.All(gu => gu.GuildId != guild.Id))
                        {
                            logger.LogInformation("User {UserId} is not associated to guild {GuildId}", user.Id,
                                guild.Id);
                            userModels[user.Id].GuildUsers.Add(new GuildUserModel
                            {
                                User = userModels[user.Id],
                                UserId = user.Id,
                                Guild = guildModels[guild.Id],
                                GuildId = guild.Id
                            });
                            logger.LogInformation("User {UserId} is associated to guild {GuildId}", user.Id, guild.Id);
                        }
                    }
                }
            }

            // Remove old entities
            foreach (var guild in guildModels.Values.Where(g => !guildIds.Contains(g.Id)))
            {
                logger.LogInformation("Removing  guild {GuildId}", guild.Id);
                uow.Guilds.Remove(guild);
            }

            foreach (var channel in channelModels.Values.Where(c => !channelIds.Contains(c.Id)))
            {
                logger.LogInformation("Removing channel {ChannelId}", channel.Id);
                uow.Channels.Remove(channel);
            }

            foreach (var user in userModels.Values.Where(u => !userIds.Contains(u.Id)))
            {
                logger.LogInformation("Removing user {UserId}", user.Id);
                uow.Users.Remove(user);
            }

            await uow.CompleteAsync();
            logger.LogInformation("Finished sync process");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in sync process");
        }
    }

    public async Task PresenceUpdated(IUser socketUser, IPresence formerSocketPresence, IPresence currentSocketPresence)
    {
        try
        {
            if (currentSocketPresence.Status == UserStatus.Online || socketUser.IsBot)
            {
                return;
            }

            var now = DateTime.UtcNow;
            logger.LogInformation("Updating user {UserId} last online to {LastOnline}", socketUser.Id, now);
            using var scope = serviceScopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var user = await uow.Users.GetById(socketUser.Id);
            if (user == null)
            {

                return;
            }

            user.LastOnline = now;
            await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing presence updated for UserId {UserId}", socketUser.Id);
        }
    }

    public async Task UserVoiceStateUpdated(IUser user, IVoiceState previousVoiceState, IVoiceState currentVoiceState)
    {
        try
        {
            logger.LogInformation("Processing user voice state updated for {UserId}", user.Id);
            using var scope = serviceScopeFactory.CreateScope();
            var hotOpService = scope.ServiceProvider.GetRequiredService<IHotOpService>();
            if (previousVoiceState.VoiceChannel == null && currentVoiceState.VoiceChannel != null)
            {
                var connectedUsers = await currentVoiceState.VoiceChannel.GetUsersAsync().FlattenAsync();
                await hotOpService.UpdateHotOps(user.Id, UserVoiceStateAction.Connected,
                    connectedUsers.Select(u => u.Id).ToList());
            }

            if (currentVoiceState.VoiceChannel == null && previousVoiceState.VoiceChannel != null)
            {
                var connectedUsers = await previousVoiceState.VoiceChannel.GetUsersAsync().FlattenAsync();
                await hotOpService.UpdateHotOps(user.Id, UserVoiceStateAction.Disconnected,
                    connectedUsers.Select(u => u.Id).ToList());
            }

            logger.LogInformation("Finished processing user voice state updated for {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing user voice state updated for {UserId}", user.Id);
        }
    }

    public async Task MessageReceived(IMessage message)
    {
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message received");
        }
    }

    public async Task MessageDeleted(IMessage message,
        IMessageChannel channel, IGuild guild)
    {
        try
        {
            logger.LogInformation("Message deleted. {Author}: {MessageText}", message.Author.Username, message.Content);
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
            logger.LogInformation("Finished processing message deletion");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Message deleted failed");
        }
    }

    public async Task ThreadCreated(IThreadChannel thread)
    {
        try
        {
            logger.LogInformation("Thread created: {ThreadId}", thread.Id);
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
                logger.LogInformation("Archiving thread {ThreadId}", thread.Id);
                existingThread.Archived = false;
            }
            else
            {
                logger.LogInformation("Creating thread {ThreadId} in database", thread.Id);
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Thread creation failed");
        }
    }

    public async Task ThreadDeleted(IThreadChannel thread)
    {
        try
        {
            logger.LogInformation("Thread deleted: {ThreadId}", thread.Id);
            using var scope = serviceScopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var existingThread = await uow.Channels.GetById(thread.Id);
            if (existingThread == null)
            {
                return;
            }

            uow.Channels.Remove(existingThread);
            await uow.CompleteAsync();
            logger.LogInformation("Finished deleting thread {ThreadId} from database", thread.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Thread deleted failed");
        }
    }

    public async Task ThreadMemberJoined(IThreadUser user)
    {
        try
        {
            if (user.GuildUser.IsBot || user.GuildUser.IsWebhook)
            {
                return;
            }

            logger.LogInformation("User {UserId} joined thread {ThreadId}", user.GuildUser.Id, user.Thread.Id);
            using var scope = serviceScopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var existingChannelModel = await uow.Channels.GetByIdWithChannelUsers(user.Thread.Id);
            var existingDiscordUser = await uow.Users.GetById(user.GuildUser.Id);

            if (existingChannelModel == null)
            {
                logger.LogWarning("Thread {ChannelId} does not exist", user.Thread.Id);
                return;
            }

            if (existingDiscordUser == null)
            {
                logger.LogWarning("User {UserId} does not exist", user.GuildUser.Id);
                return;
            }


            if (existingChannelModel.ChannelUsers.All(cu => cu.UserId != user.GuildUser.Id))
            {
                logger.LogInformation("Adding user {UserId} to thread {ThreadId}", user.GuildUser.Id, user.Thread.Id);
                existingChannelModel.ChannelUsers.Add(new ChannelUserModel
                {
                    Channel = existingChannelModel,
                    ChannelId = existingChannelModel.Id,
                    User = existingDiscordUser,
                    UserId = existingDiscordUser.Id
                });
                await uow.CompleteAsync();
            }

            logger.LogInformation("Finished processing user {UserId} joining thread {ThreadId}", user.GuildUser.Id,
                user.Thread.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Thread member joined failed");
        }
    }

    public async Task ThreadUpdated(IThreadChannel oldThread, IThreadChannel newThread)
    {
        try
        {
            logger.LogInformation("Thread updated: {ThreadId}", oldThread.Id);
            using var scope = serviceScopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var existingChannelModel = await uow.Channels.GetById(oldThread.Id);
            if (existingChannelModel == null || (existingChannelModel.Name == newThread.Name &&
                                                 existingChannelModel.Archived == newThread.IsArchived))
            {
                return;
            }

            existingChannelModel.Name = newThread.Name;
            existingChannelModel.Archived = newThread.IsArchived;
            await uow.CompleteAsync();
            logger.LogInformation("Finished processing thread {ThreadId}", oldThread.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Thread updated failed");
        }
    }

    public async Task ThreadMemberLeft(IThreadUser threadUser)
    {
        try
        {
            if (threadUser.GuildUser.IsBot)
            {
                return;
            }

            logger.LogInformation("User {UserId} left thread {ThreadId}", threadUser.GuildUser.Id,
                threadUser.Thread.Id);
            using var scope = serviceScopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var existingChannelModel = await uow.Channels.GetByIdWithChannelUsers(threadUser.Thread.Id);
            var existingDiscordUser = await uow.Users.GetById(threadUser.GuildUser.Id);

            if (existingChannelModel == null)
            {
                logger.LogWarning("Thread {ThreadId} does not exist", threadUser.Thread.Id);
                return;
            }

            if (existingDiscordUser == null)
            {
                logger.LogWarning("User {UserId} does not exist", threadUser.GuildUser.Id);
                return;
            }

            var channelUserModel =
                existingChannelModel.ChannelUsers.FirstOrDefault(cu => cu.UserId == existingDiscordUser.Id);
            if (channelUserModel == null)
            {
                logger.LogInformation("User was not a part of the thread");
                return;
            }

            existingChannelModel.ChannelUsers.Remove(channelUserModel);
            await uow.CompleteAsync();
            logger.LogInformation("Finished processing user {UserId} leaving thread {ThreadId}",
                threadUser.GuildUser.Id, threadUser.Thread.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Thread member left failed");
        }
    }
}