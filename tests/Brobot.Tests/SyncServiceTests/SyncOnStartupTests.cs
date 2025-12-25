using Brobot.Configuration;
using Brobot.Models;
using Brobot.Services;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class SyncOnStartupTests : SyncServiceTestsBase
{
    protected override void SetupDatabase()
    {
        GuildModel guild = new()
        {
            Id = 1UL,
            Name = "test-guild1"
        };
        Context.Guilds.Add(guild);
        ChannelModel channel1 = new()
        {
            Id = 1UL,
            Name = "test-channel1",
            Guild = guild,
            GuildId = guild.Id
        };
        ChannelModel channel2 = new()
        {
            Id = 2UL,
            Name = "test-channel2",
            Guild = guild,
            GuildId = guild.Id
        };
        Context.Channels.AddRange(channel1, channel2);

        DiscordUserModel user1 = new()
        {
            Id = 1UL,
            Username = "test-user1"
        };
        DiscordUserModel user2 = new()
        {
            Id = 2UL,
            Username = "test-user2"
        };
        DiscordUserModel user3 = new()
        {
            Id = 3UL,
            Username = "test-user3"
        };
        DiscordUserModel user4 = new()
        {
            Id = 4UL,
            Username = "test-user4"
        };
        Context.DiscordUsers.AddRange(user1, user2, user3, user4);

        guild.GuildDiscordUsers.Add(new GuildDiscordUserModel
        {
            Guild = guild,
            GuildId = guild.Id,
            DiscordUser = user1,
            DiscordUserId = user1.Id
        });
        guild.GuildDiscordUsers.Add(new GuildDiscordUserModel
        {
            Guild = guild,
            GuildId = guild.Id,
            DiscordUser = user2,
            DiscordUserId = user2.Id
        });
        guild.GuildDiscordUsers.Add(new GuildDiscordUserModel
        {
            Guild = guild,
            GuildId = guild.Id,
            DiscordUser = user3,
            DiscordUserId = user3.Id
        });
        guild.GuildDiscordUsers.Add(new GuildDiscordUserModel
        {
            Guild = guild,
            GuildId = guild.Id,
            DiscordUser = user4,
            DiscordUserId = user4.Id
        });
        
        channel1.ChannelUsers.Add(new ChannelDiscordUserModel
        {
            Channel = channel1,
            ChannelId = channel1.Id,
            DiscordUser = user1,
            UserId = user1.Id
        });
        channel1.ChannelUsers.Add(new ChannelDiscordUserModel
        {
            Channel = channel1,
            ChannelId = channel1.Id,
            DiscordUser = user2,
            UserId = user2.Id
        });
        channel2.ChannelUsers.Add(new ChannelDiscordUserModel
        {
            Channel = channel2,
            ChannelId = channel2.Id,
            DiscordUser = user3,
            UserId = user3.Id
        });
        channel2.ChannelUsers.Add(new ChannelDiscordUserModel
        {
            Channel = channel2,
            ChannelId = channel2.Id,
            DiscordUser = user4,
            UserId = user4.Id
        });
        
        Context.SaveChanges();
    }

    [Test]
    public async Task GuildsChannelsUsersMatch_NoChanges()
    {
        const ulong guildId = 1UL;
        const ulong channel1Id = 1UL;
        const ulong channel2Id = 2UL;
        const ulong user1Id = 1UL;
        const ulong user2Id = 2UL;
        const ulong user3Id = 3UL;
        const ulong user4Id = 4UL;
        const string guildName = "test-guild1";
        const string channel1Name = "test-channel1";
        const string channel2Name = "test-channel2";
        const string user1Name = "test-user1";
        const string user2Name = "test-user2";
        const string user3Name = "test-user3";
        const string user4Name = "test-user4";
        var user1Mock = SetupUserMock(user1Id, user1Name);
        var user2Mock = SetupUserMock(user2Id, user2Name);
        var user3Mock = SetupUserMock(user3Id, user3Name);
        var user4Mock = SetupUserMock(user4Id, user4Name);
        var channel1Mock = SetupChannelMock(channel1Id, channel1Name, [user1Mock.Object, user2Mock.Object]);
        var channel2Mock = SetupChannelMock(channel2Id, channel2Name, [user3Mock.Object, user4Mock.Object]);
        var guildMock = SetupGuildMock(guildId, guildName, [channel1Mock.Object, channel2Mock.Object]);
        MockDiscordClient.Setup(dc => dc.GetGuildsAsync(
            CacheMode.AllowDownload,
            null)).ReturnsAsync([guildMock.Object]);

        await SyncService.SyncOnStartup();
        
        Context.ChangeTracker.Clear();
        var guildModel = await Context.Guilds
            .AsSplitQuery()
            .Include(g => g.Channels)
            .ThenInclude(c => c.ChannelUsers)
            .Include(g => g.GuildDiscordUsers)
            .SingleOrDefaultAsync(g => g.Id == guildId);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.Id, Is.EqualTo(guildId));
            Assert.That(guildModel.Name, Is.EqualTo(guildName));
            var channel1Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel1Id);
            Assert.That(channel1Model, Is.Not.Null);
            Assert.That(channel1Model!.Name, Is.EqualTo(channel1Name));
            Assert.That(channel1Model.ChannelUsers, Has.Count.EqualTo(2));
            var channel2Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel2Id);
            Assert.That(channel2Model, Is.Not.Null);
            Assert.That(channel2Model!.Name, Is.EqualTo(channel2Name));
            Assert.That(channel2Model.ChannelUsers, Has.Count.EqualTo(2));
        }
    }

    [Test]
    [TestCase("updated-guild", "test-channel1", "test-channel2", "test-user1",  "test-user2", "test-user3", "test-user4")]
    [TestCase("test-guild1", "updated-channel1", "test-channel2", "test-user1",  "test-user2", "test-user3", "test-user4")]
    [TestCase("test-guild1", "test-channel1", "test-channel2", "updated-user1",  "test-user2", "test-user3", "test-user4")]
    public async Task NameChanged_UpdatedInDatabase(
        string guildName,
        string channel1Name,
        string channel2Name,
        string user1Name,
        string user2Name,
        string user3Name,
        string user4Name)
    {
        const ulong guildId = 1UL;
        const ulong channel1Id = 1UL;
        const ulong channel2Id = 2UL;
        const ulong user1Id = 1UL;
        const ulong user2Id = 2UL;
        const ulong user3Id = 3UL;
        const ulong user4Id = 4UL;
        var user1Mock = SetupUserMock(user1Id, user1Name);
        var user2Mock = SetupUserMock(user2Id, user2Name);
        var user3Mock = SetupUserMock(user3Id, user3Name);
        var user4Mock = SetupUserMock(user4Id, user4Name);
        var channel1Mock = SetupChannelMock(channel1Id, channel1Name, [user1Mock.Object, user2Mock.Object]);
        var channel2Mock = SetupChannelMock(channel2Id, channel2Name, [user3Mock.Object, user4Mock.Object]);
        var guildMock = SetupGuildMock(guildId, guildName, [channel1Mock.Object, channel2Mock.Object]);
        MockDiscordClient.Setup(dc => dc.GetGuildsAsync(
            CacheMode.AllowDownload,
            null)).ReturnsAsync([guildMock.Object]);

        await SyncService.SyncOnStartup();
        
        Context.ChangeTracker.Clear();
        var guildModel = await Context.Guilds
            .AsSplitQuery()
            .Include(g => g.Channels)
            .ThenInclude(c => c.ChannelUsers)
            .Include(g => g.GuildDiscordUsers)
            .SingleOrDefaultAsync(g => g.Id == guildId);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.Id, Is.EqualTo(guildId));
            Assert.That(guildModel.Name, Is.EqualTo(guildName));
            var channel1Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel1Id);
            Assert.That(channel1Model, Is.Not.Null);
            Assert.That(channel1Model!.Name, Is.EqualTo(channel1Name));
            Assert.That(channel1Model.ChannelUsers, Has.Count.EqualTo(2));
            var channel2Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel2Id);
            Assert.That(channel2Model, Is.Not.Null);
            Assert.That(channel2Model!.Name, Is.EqualTo(channel2Name));
            Assert.That(channel2Model.ChannelUsers, Has.Count.EqualTo(2));
        }
    }

    [Test]
    public async Task UserRemoved_UpdatedInDatabase()
    {
        const ulong guildId = 1UL;
        const ulong channel1Id = 1UL;
        const ulong channel2Id = 2UL;
        const ulong user1Id = 1UL;
        const ulong user2Id = 2UL;
        const ulong user3Id = 3UL;
        const ulong removedUserId = 4UL;
        const string guildName = "test-guild1";
        const string channel1Name = "test-channel1";
        const string channel2Name = "test-channel2";
        const string user1Name = "test-user1";
        const string user2Name = "test-user2";
        const string user3Name = "test-user3";
        var user1Mock = SetupUserMock(user1Id, user1Name);
        var user2Mock = SetupUserMock(user2Id, user2Name);
        var user3Mock = SetupUserMock(user3Id, user3Name);
        var channel1Mock = SetupChannelMock(channel1Id, channel1Name, [user1Mock.Object, user2Mock.Object]);
        var channel2Mock = SetupChannelMock(channel2Id, channel2Name, [user3Mock.Object]);
        var guildMock = SetupGuildMock(guildId, guildName, [channel1Mock.Object, channel2Mock.Object]);
        MockDiscordClient.Setup(dc => dc.GetGuildsAsync(
            CacheMode.AllowDownload,
            null)).ReturnsAsync([guildMock.Object]);

        await SyncService.SyncOnStartup();
        
        Context.ChangeTracker.Clear();
        var guildModel = await Context.Guilds
            .AsSplitQuery()
            .Include(g => g.Channels)
            .ThenInclude(c => c.ChannelUsers)
            .Include(g => g.GuildDiscordUsers)
            .SingleOrDefaultAsync(g => g.Id == guildId);
        var removedUserModel = await Context.DiscordUsers.FindAsync(removedUserId);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(removedUserModel, Is.Not.Null);
            Assert.That(removedUserModel!.Archived, Is.True);
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.Id, Is.EqualTo(guildId));
            Assert.That(guildModel.Name, Is.EqualTo(guildName));
            var channel1Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel1Id);
            Assert.That(channel1Model, Is.Not.Null);
            Assert.That(channel1Model!.Name, Is.EqualTo(channel1Name));
            Assert.That(channel1Model.ChannelUsers, Has.Count.EqualTo(2));
            var channel2Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel2Id);
            Assert.That(channel2Model, Is.Not.Null);
            Assert.That(channel2Model!.Name, Is.EqualTo(channel2Name));
            Assert.That(channel2Model.ChannelUsers, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task ChannelRemoved_UpdatedInDatabase()
    {
        const ulong guildId = 1UL;
        const ulong channel1Id = 1UL;
        const ulong removedChannelId = 2UL;
        const ulong user1Id = 1UL;
        const ulong user2Id = 2UL;
        const string guildName = "test-guild1";
        const string channel1Name = "test-channel1";
        const string user1Name = "test-user1";
        const string user2Name = "test-user2";
        var user1Mock = SetupUserMock(user1Id, user1Name);
        var user2Mock = SetupUserMock(user2Id, user2Name);
        var channel1Mock = SetupChannelMock(channel1Id, channel1Name, [user1Mock.Object, user2Mock.Object]);
        var guildMock = SetupGuildMock(guildId, guildName, [channel1Mock.Object]);
        MockDiscordClient.Setup(dc => dc.GetGuildsAsync(
            CacheMode.AllowDownload,
            null)).ReturnsAsync([guildMock.Object]);

        await SyncService.SyncOnStartup();
        
        Context.ChangeTracker.Clear();
        var guildModel = await Context.Guilds
            .AsSplitQuery()
            .Include(g => g.Channels)
            .ThenInclude(c => c.ChannelUsers)
            .Include(g => g.GuildDiscordUsers)
            .SingleOrDefaultAsync(g => g.Id == guildId);
        var removedChannelModel = await Context.Channels.FindAsync(removedChannelId);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(removedChannelModel, Is.Not.Null);
            Assert.That(removedChannelModel!.Archived, Is.True);
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.Id, Is.EqualTo(guildId));
            Assert.That(guildModel.Name, Is.EqualTo(guildName));
            var channel1Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel1Id);
            Assert.That(channel1Model, Is.Not.Null);
            Assert.That(channel1Model!.Name, Is.EqualTo(channel1Name));
            Assert.That(channel1Model.ChannelUsers, Has.Count.EqualTo(2));
        }
    }

    [Test]
    public async Task GuildRemoved_UpdatedInDatabase()
    {
        const ulong removedGuildId = 1UL;
        MockDiscordClient.Setup(dc => dc.GetGuildsAsync(
            CacheMode.AllowDownload,
            null)).ReturnsAsync([]);
        
        await SyncService.SyncOnStartup();
        
        Context.ChangeTracker.Clear();
        var guildModel = await Context.Guilds
            .AsSplitQuery()
            .Include(g => g.Channels)
            .ThenInclude(c => c.ChannelUsers)
            .Include(g => g.GuildDiscordUsers)
            .SingleOrDefaultAsync(g => g.Id == removedGuildId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.Archived, Is.True);
            Assert.That(guildModel.Channels.All(c => c.Archived), Is.True);
        }
    }

    [Test]
    public async Task UserAdded_UpdatedInDatabase()
    {
        const ulong guildId = 1UL;
        const ulong channel1Id = 1UL;
        const ulong channel2Id = 2UL;
        const ulong user1Id = 1UL;
        const ulong user2Id = 2UL;
        const ulong user3Id = 3UL;
        const ulong user4Id = 4UL;
        const ulong  user5Id = 5UL;
        const string guildName = "test-guild1";
        const string channel1Name = "test-channel1";
        const string channel2Name = "test-channel2";
        const string user1Name = "test-user1";
        const string user2Name = "test-user2";
        const string user3Name = "test-user3";
        const string user4Name = "test-user4";
        const string user5Name = "test-user5";
        var user1Mock = SetupUserMock(user1Id, user1Name);
        var user2Mock = SetupUserMock(user2Id, user2Name);
        var user3Mock = SetupUserMock(user3Id, user3Name);
        var user4Mock = SetupUserMock(user4Id, user4Name);
        var user5Mock = SetupUserMock(user5Id, user5Name);
        var channel1Mock = SetupChannelMock(channel1Id, channel1Name, [user1Mock.Object, user2Mock.Object, user5Mock.Object]);
        var channel2Mock = SetupChannelMock(channel2Id, channel2Name, [user3Mock.Object, user4Mock.Object]);
        var guildMock = SetupGuildMock(guildId, guildName, [channel1Mock.Object, channel2Mock.Object]);
        MockDiscordClient.Setup(dc => dc.GetGuildsAsync(
            CacheMode.AllowDownload,
            null)).ReturnsAsync([guildMock.Object]);

        await SyncService.SyncOnStartup();
        
        Context.ChangeTracker.Clear();
        var guildModel = await Context.Guilds
            .AsSplitQuery()
            .Include(g => g.Channels)
            .ThenInclude(c => c.ChannelUsers)
            .Include(g => g.GuildDiscordUsers)
            .SingleOrDefaultAsync(g => g.Id == guildId);
        var newUserModel = await Context.DiscordUsers.FindAsync(user5Id);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(newUserModel, Is.Not.Null);
            Assert.That(newUserModel!.Username, Is.EqualTo(user5Name));
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.Id, Is.EqualTo(guildId));
            Assert.That(guildModel.Name, Is.EqualTo(guildName));
            var channel1Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel1Id);
            Assert.That(channel1Model, Is.Not.Null);
            Assert.That(channel1Model!.Name, Is.EqualTo(channel1Name));
            Assert.That(channel1Model.ChannelUsers, Has.Count.EqualTo(3));
            var channel2Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel2Id);
            Assert.That(channel2Model, Is.Not.Null);
            Assert.That(channel2Model!.Name, Is.EqualTo(channel2Name));
            Assert.That(channel2Model.ChannelUsers, Has.Count.EqualTo(2));
        }
    }

    [Test]
    public async Task ChannelAdded_UpdatedInDatabase()
    {
        const ulong guildId = 1UL;
        const ulong channel1Id = 1UL;
        const ulong channel2Id = 2UL;
        const ulong newChannelId = 3UL;
        const ulong user1Id = 1UL;
        const ulong user2Id = 2UL;
        const ulong user3Id = 3UL;
        const ulong user4Id = 4UL;
        const string guildName = "test-guild1";
        const string channel1Name = "test-channel1";
        const string channel2Name = "test-channel2";
        const string newChannelName = "test-channel3";
        const string user1Name = "test-user1";
        const string user2Name = "test-user2";
        const string user3Name = "test-user3";
        const string user4Name = "test-user4";
        var user1Mock = SetupUserMock(user1Id, user1Name);
        var user2Mock = SetupUserMock(user2Id, user2Name);
        var user3Mock = SetupUserMock(user3Id, user3Name);
        var user4Mock = SetupUserMock(user4Id, user4Name);
        var channel1Mock = SetupChannelMock(channel1Id, channel1Name, [user1Mock.Object, user2Mock.Object]);
        var channel2Mock = SetupChannelMock(channel2Id, channel2Name, [user3Mock.Object, user4Mock.Object]);
        var channel3Mock = SetupChannelMock(newChannelId, newChannelName, [user1Mock.Object, user2Mock.Object, user3Mock.Object]);
        var guildMock = SetupGuildMock(guildId, guildName, [channel1Mock.Object, channel2Mock.Object, channel3Mock.Object]);
        MockDiscordClient.Setup(dc => dc.GetGuildsAsync(
            CacheMode.AllowDownload,
            null)).ReturnsAsync([guildMock.Object]);

        await SyncService.SyncOnStartup();
        
        Context.ChangeTracker.Clear();
        var guildModel = await Context.Guilds
            .AsSplitQuery()
            .Include(g => g.Channels)
            .ThenInclude(c => c.ChannelUsers)
            .Include(g => g.GuildDiscordUsers)
            .SingleOrDefaultAsync(g => g.Id == guildId);
        var newChannelModel = await Context.Channels
            .Include(c => c.ChannelUsers)
            .SingleOrDefaultAsync(c => c.Id == newChannelId);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(newChannelModel, Is.Not.Null);
            Assert.That(newChannelModel!.Name, Is.EqualTo(newChannelName));
            Assert.That(newChannelModel.ChannelUsers, Has.Count.EqualTo(3));
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.Id, Is.EqualTo(guildId));
            Assert.That(guildModel.Name, Is.EqualTo(guildName));
            Assert.That(guildModel.Channels, Has.Count.EqualTo(3));
            var channel1Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel1Id);
            Assert.That(channel1Model, Is.Not.Null);
            Assert.That(channel1Model!.Name, Is.EqualTo(channel1Name));
            Assert.That(channel1Model.ChannelUsers, Has.Count.EqualTo(2));
            var channel2Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel2Id);
            Assert.That(channel2Model, Is.Not.Null);
            Assert.That(channel2Model!.Name, Is.EqualTo(channel2Name));
            Assert.That(channel2Model.ChannelUsers, Has.Count.EqualTo(2));
        }
    }

    [Test]
    public async Task GuildAdded_UpdatedInDatabase()
    {
        const ulong guildId = 1UL;
        const ulong newGuildId = 2UL;
        const ulong channel1Id = 1UL;
        const ulong channel2Id = 2UL;
        const ulong newChannelId = 3UL;
        const ulong user1Id = 1UL;
        const ulong user2Id = 2UL;
        const ulong user3Id = 3UL;
        const ulong user4Id = 4UL;
        const ulong newUserId = 5UL;
        const string guildName = "test-guild1";
        const string newGuildName = "test-guild2";
        const string channel1Name = "test-channel1";
        const string channel2Name = "test-channel2";
        const string newChannelName = "test-channel3";
        const string user1Name = "test-user1";
        const string user2Name = "test-user2";
        const string user3Name = "test-user3";
        const string user4Name = "test-user4";
        const string newUserName = "test-user5";
        var user1Mock = SetupUserMock(user1Id, user1Name);
        var user2Mock = SetupUserMock(user2Id, user2Name);
        var user3Mock = SetupUserMock(user3Id, user3Name);
        var user4Mock = SetupUserMock(user4Id, user4Name);
        var newUserMock =  SetupUserMock(newUserId, newUserName);
        var channel1Mock = SetupChannelMock(channel1Id, channel1Name, [user1Mock.Object, user2Mock.Object]);
        var channel2Mock = SetupChannelMock(channel2Id, channel2Name, [user3Mock.Object, user4Mock.Object]);
        var newChannelMock = SetupChannelMock(newChannelId, newChannelName, [newUserMock.Object]);
        var guildMock = SetupGuildMock(guildId, guildName, [channel1Mock.Object, channel2Mock.Object]);
        var newGuildMock = SetupGuildMock(newGuildId, newGuildName, [newChannelMock.Object]);
        MockDiscordClient.Setup(dc => dc.GetGuildsAsync(
            CacheMode.AllowDownload,
            null)).ReturnsAsync([guildMock.Object, newGuildMock.Object]);

        await SyncService.SyncOnStartup();
        
        Context.ChangeTracker.Clear();
        var guildCount = await Context.Guilds.CountAsync();
        var newGuildModel = await Context.Guilds
            .AsSplitQuery()
            .Include(g => g.Channels)
            .ThenInclude(c => c.ChannelUsers)
            .Include(g => g.GuildDiscordUsers)
            .SingleOrDefaultAsync(g => g.Id == newGuildId);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildCount, Is.EqualTo(2));
            Assert.That(newGuildModel, Is.Not.Null);
            Assert.That(newGuildModel!.Name, Is.EqualTo(newGuildName));
            Assert.That(newGuildModel.Channels, Has.Count.EqualTo(1));
            Assert.That(newGuildModel.GuildDiscordUsers, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task ChannelIsNotText_NotAddedToDatabase()
    {
        const ulong guildId = 1UL;
        const ulong channel1Id = 1UL;
        const ulong channel2Id = 2UL;
        const ulong voiceChannelId = 3UL;
        const ulong user1Id = 1UL;
        const ulong user2Id = 2UL;
        const ulong user3Id = 3UL;
        const ulong user4Id = 4UL;
        const string guildName = "test-guild1";
        const string channel1Name = "test-channel1";
        const string channel2Name = "test-channel2";
        const string voiceChannelName = "test-voiceChannel";
        const string user1Name = "test-user1";
        const string user2Name = "test-user2";
        const string user3Name = "test-user3";
        const string user4Name = "test-user4";
        var user1Mock = SetupUserMock(user1Id, user1Name);
        var user2Mock = SetupUserMock(user2Id, user2Name);
        var user3Mock = SetupUserMock(user3Id, user3Name);
        var user4Mock = SetupUserMock(user4Id, user4Name);
        var channel1Mock = SetupChannelMock(channel1Id, channel1Name, [user1Mock.Object, user2Mock.Object]);
        var channel2Mock = SetupChannelMock(channel2Id, channel2Name, [user3Mock.Object, user4Mock.Object]);
        var voiceChannelMock = SetupChannelMock(voiceChannelId, voiceChannelName, [user3Mock.Object, user4Mock.Object], ChannelType.Voice);
        var guildMock = SetupGuildMock(guildId, guildName, [channel1Mock.Object, channel2Mock.Object, voiceChannelMock.Object]);
        MockDiscordClient.Setup(dc => dc.GetGuildsAsync(
            CacheMode.AllowDownload,
            null)).ReturnsAsync([guildMock.Object]);

        await SyncService.SyncOnStartup();
        
        Context.ChangeTracker.Clear();
        var guildModel = await Context.Guilds
            .AsSplitQuery()
            .Include(g => g.Channels)
            .ThenInclude(c => c.ChannelUsers)
            .Include(g => g.GuildDiscordUsers)
            .SingleOrDefaultAsync(g => g.Id == guildId);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.Id, Is.EqualTo(guildId));
            Assert.That(guildModel.Name, Is.EqualTo(guildName));
            var channel1Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel1Id);
            Assert.That(channel1Model, Is.Not.Null);
            Assert.That(channel1Model!.Name, Is.EqualTo(channel1Name));
            Assert.That(channel1Model.ChannelUsers, Has.Count.EqualTo(2));
            var channel2Model = guildModel.Channels.FirstOrDefault(c => c.Id == channel2Id);
            Assert.That(channel2Model, Is.Not.Null);
            Assert.That(channel2Model!.Name, Is.EqualTo(channel2Name));
            Assert.That(channel2Model.ChannelUsers, Has.Count.EqualTo(2));
        }
    }

    [Test]
    public async Task ExceptionOccurs_LogsErrorMessage()
    {
        Mock<IServiceScopeFactory> mockServiceScopeFactory = new();
        mockServiceScopeFactory.Setup(sp => sp.CreateScope())
            .Throws(new Exception("Test exception"));
        var syncServiceWithException = new SyncService(
            mockServiceScopeFactory.Object,
            MockDiscordClient.Object,
            LoggerMock.Object,
            Options.Create(new GeneralOptions
            {
                SeqUrl = "http://localhost:5341",
                VersionFilePath = "./version.txt"
            }));
        
        await syncServiceWithException.SyncOnStartup();
        
        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error in sync process")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    private Mock<IGuildUser> SetupUserMock(ulong userId, string username)
    {
        Mock<IGuildUser> mockUser = new();
        mockUser.SetupGet(u => u.Username).Returns(username);
        mockUser.SetupGet(u => u.Id).Returns(userId);
        return mockUser;
    }

    private Mock<IGuildChannel> SetupChannelMock(ulong channelId, string channelName,
        IReadOnlyCollection<IGuildUser> users, ChannelType channelType = ChannelType.Text)
    {
        Mock<IGuildChannel> mockChannel = new();
        mockChannel.SetupGet(c => c.Name).Returns(channelName);
        mockChannel.SetupGet(c => c.Id).Returns(channelId);
        mockChannel.SetupGet(c => c.ChannelType).Returns(channelType);
        List<IReadOnlyCollection<IGuildUser>> usersCollection = [users];
        mockChannel.Setup(c => c.GetUsersAsync(
            CacheMode.AllowDownload,
            null)).Returns(usersCollection.ToAsyncEnumerable());
        return mockChannel;
    }

    private Mock<IGuild> SetupGuildMock(ulong guildId, string name, IReadOnlyCollection<IGuildChannel> channels)
    {
        Mock<IGuild> mockGuild = new();
        mockGuild.SetupGet(g => g.Name).Returns(name);
        mockGuild.SetupGet(g => g.Id).Returns(guildId);
        mockGuild.Setup(g => g.GetChannelsAsync(
            CacheMode.AllowDownload,
            null)).ReturnsAsync(channels);
        return mockGuild;
    }
}