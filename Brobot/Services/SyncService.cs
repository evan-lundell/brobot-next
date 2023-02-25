using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Discord.WebSocket;

namespace Brobot.Services;

public class SyncService : ISyncService
{
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;

    public SyncService(IServiceProvider services, DiscordSocketClient client)
    {
        _services = services;
        _client = client;
    }

    public async Task ChannelCreated(SocketTextChannel channel)
    {
        using (var scope = _services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var guild = await uow.Guilds.GetById(channel.Guild.Id);
            if (guild == null)
            {
                return;
            }
            var userModels = (await uow.Users.GetAllWithGuildsAndChannels())
                .Where((u) => u.Archived == false)
                .ToDictionary((u) => u.Id);
            var newChannel = new ChannelModel
            {
                Id = channel.Id,
                Name = channel.Name,
                Guild = guild,
                GuildId = guild.Id
            };

            foreach (var user in channel.Users)
            {
                newChannel.ChannelUsers.Add(new ChannelUserModel
                {
                    Channel = newChannel,
                    ChannelId = channel.Id,
                    User = userModels[user.Id],
                    UserId = user.Id
                });
            }

            await uow.CompleteAsync();
        }
    }

    public async Task ChannelDestroyed(SocketTextChannel channel)
    {
        using (var scope = _services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var channelToBeDeleted = await uow.Channels.GetById(channel.Id);
            if (channelToBeDeleted == null)
            {
                return;
            }

            uow.Channels.Remove(channelToBeDeleted);
            await uow.CompleteAsync();
        }
    }

    public async Task ChannelUpdated(SocketTextChannel previous, SocketTextChannel current)
    {
        if (previous.Name == current.Name)
        {
            return;
        }

        var guild = _client.GetGuild(current.Guild.Id);
        var userName = "";
        var auditLog = (await guild.GetAuditLogsAsync(limit: 1, actionType: Discord.ActionType.ChannelUpdated).FlattenAsync()).First();

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

        using (var scope = _services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var channelModel = await uow.Channels.GetById(current.Id);
            if (channelModel == null)
            {
                return;
            }
            channelModel.Name = current.Name;
            await uow.CompleteAsync();
        }
    }

    public async Task GuildAvailable(SocketGuild guild)
    {
        using (var scope = _services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var guildModel = await uow.Guilds.GetById(guild.Id);
            if (guildModel != null)
            {
                return;
            }

            var userIds = (await uow.Users.GetAllWithGuildsAndChannels())
                .Where((u) => u.Archived == false)
                .ToDictionary((u) => u.Id);
            var newGuild = new GuildModel
            {
                Id = guild.Id,
                Name = guild.Name
            };
            await uow.Guilds.Add(newGuild);

            foreach (var channel in guild.Channels.Where((c) => c is SocketTextChannel && !(c is SocketVoiceChannel)))
            {
                var newChannel = new ChannelModel
                {
                    Id = channel.Id,
                    Name = channel.Name,
                    Guild = newGuild,
                    GuildId = guild.Id
                };
                await uow.Channels.Add(newChannel);

                var userAddedToGuild = false;
                foreach (var user in channel.Users.Where((u) => !u.IsBot))
                {
                    if (!userIds.ContainsKey(user.Id))
                    {
                        var newUser = new UserModel
                        {
                            Id = user.Id,
                            Username = user.Username
                        };
                        userIds.Add(user.Id, newUser);
                    }

                    if (!userAddedToGuild)
                    {
                        newGuild.GuildUsers.Add(new GuildUserModel
                        {
                            Guild = newGuild,
                            GuildId = guild.Id,
                            User = userIds[user.Id],
                            UserId = user.Id
                        });
                        userAddedToGuild = true;
                    }

                    newChannel.ChannelUsers.Add(new ChannelUserModel
                    {
                        Channel = newChannel,
                        ChannelId = channel.Id,
                        User = userIds[user.Id],
                        UserId = user.Id
                    });
                }
            }

            await uow.CompleteAsync();
        }
    }

    public async Task GuildUnavailable(SocketGuild guild)
    {
        using (var scope = _services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var guildToBeDeleted = await uow.Guilds.GetById(guild.Id);
            if (guildToBeDeleted == null)
            {
                return;
            }
            uow.Guilds.Remove(guildToBeDeleted);
            await uow.CompleteAsync();
        }
    }

    public async Task GuildUpdated(SocketGuild previousGuild, SocketGuild currentGuild)
    {
        if (previousGuild.Name == currentGuild.Name)
        {
            return;
        }

        using (var scope = _services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var guildModel = await uow.Guilds.GetById(currentGuild.Id);
            if (guildModel == null)
            {
                return;
            }
            guildModel.Name = currentGuild.Name;
            await uow.CompleteAsync();
        }
    }

    public async Task SyncOnStartup()
    {
        using (var scope = _services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var client = _services.GetRequiredService<DiscordSocketClient>();
            var guildModels = (await uow.Guilds.Find((g) => g.Archived == false)).ToDictionary((g) => g.Id);
            var channelModels = (await uow.Channels.Find((c) => c.Archived == false)).ToDictionary((c) => c.Id);
            var userModels = (await uow.Users.GetAllWithGuildsAndChannels()).Where((u) => u.Archived == false).ToDictionary((u) => u.Id);

            var guildIds = new HashSet<ulong>();
            var channelIds = new HashSet<ulong>();
            var userIds = new HashSet<ulong>();

            // Create new entities or update existing entities
            foreach (var guild in client.Guilds)
            {
                guildIds.Add(guild.Id);
                if (!guildModels.ContainsKey(guild.Id))
                {
                    var newGuild = new GuildModel
                    {
                        Id = guild.Id,
                        Name = guild.Name
                    };
                    await uow.Guilds.Add(newGuild);
                    guildModels.Add(guild.Id, newGuild);
                }
                else if (guildModels[guild.Id].Name != guild.Name)
                {
                    guildModels[guild.Id].Name = guild.Name;
                }

                foreach (var channel in guild.Channels)
                {
                    if (channel is SocketVoiceChannel || !(channel is SocketTextChannel))
                    {
                        continue;
                    }
                    channelIds.Add(channel.Id);
                    if (!channelModels.ContainsKey(channel.Id))
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
                    else if (channelModels[channel.Id].Name != channel.Name)
                    {
                        channelModels[channel.Id].Name = channel.Name;
                    }

                    foreach (var user in channel.Users)
                    {
                        if (user.IsBot)
                        {
                            continue;
                        }
                        if (!userIds.Contains(user.Id))
                        {
                            userIds.Add(user.Id);
                        }
                        if (!userModels.ContainsKey(user.Id))
                        {
                            var newUser = new UserModel
                            {
                                Id = user.Id,
                                Username = user.Username
                            };
                            userModels.Add(user.Id, newUser);
                            await uow.Users.Add(newUser);
                        }
                        else if (userModels[user.Id].Username != user.Username)
                        {
                            userModels[user.Id].Username = user.Username;
                        }

                        if (!userModels[user.Id].ChannelUsers.Any((cu) => cu.ChannelId == channel.Id))
                        {
                            userModels[user.Id].ChannelUsers.Add(new ChannelUserModel
                            {
                                User = userModels[user.Id],
                                UserId = user.Id,
                                Channel = channelModels[channel.Id],
                                ChannelId = channel.Id
                            });
                        }

                        if (!userModels[user.Id].GuildUsers.Any((gu) => gu.GuildId == guild.Id))
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
            foreach (var guild in guildModels.Values.Where((g) => !guildIds.Contains(g.Id)))
            {
                uow.Guilds.Remove(guild);
            }

            foreach (var channel in channelModels.Values.Where((c) => !channelIds.Contains(c.Id)))
            {
                uow.Channels.Remove(channel);
            }

            foreach (var user in userModels.Values.Where((u) => !userIds.Contains(u.Id)))
            {
                uow.Users.Remove(user);
            }

            await uow.CompleteAsync();
        }
    }

    public async Task PresenceUpdated(SocketUser socketUser, SocketPresence formerSocketPresence, SocketPresence currentSocketPresence)
    {
        if (currentSocketPresence.Status == UserStatus.Online || socketUser.IsBot)
        {
            return;
        }
        using (var scope = _services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var user = await uow.Users.GetById(socketUser.Id);
            if (user == null)
            {
                return;
            }

            user.LastOnline = DateTime.UtcNow;
            await uow.CompleteAsync();
        }
    }
}