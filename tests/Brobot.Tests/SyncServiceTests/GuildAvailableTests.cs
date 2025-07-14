using Brobot.Models;
using Discord;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Brobot.Tests.SyncServiceTests;

public class GuildAvailableTests : SyncServiceTestsBase
{
    protected override void SetupDatabase()
    {
        GuildModel guild = new()
        {
            Id = 1UL,
            Name = "test-guild"
        };
        Context.Guilds.Add(guild);
        UserModel user = new()
        {
            Id = 1UL,
            Username = "test-user"
        };
        Context.Users.Add(user);
        Context.SaveChanges();
    }

    [Test]
    public async Task GuildAlreadyExists_NothingChanged()
    {
        const ulong guildId = 1UL;
        Mock<IGuild>  guildMock = new();
        guildMock.SetupGet(g => g.Id).Returns(guildId);
        
        await SyncService.GuildAvailable(guildMock.Object);

        var guildModel = await Context.Guilds.FindAsync(guildId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildModel, Is.Not.Null);
            guildMock.Verify(g => g.GetChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()), Times.Never);
        }
    }

    [Test]
    public async Task GuildNotInDatabase_NewGuildCreated()
    {
        const ulong guildId = 2UL;
        const string guildName = "new-guild";
        Mock<IGuild> guildMock = new();
        guildMock.SetupGet(g => g.Id).Returns(guildId);
        guildMock.SetupGet(g => g.Name).Returns(guildName);
        guildMock.Setup(g => g.GetChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync([]);
        
        await SyncService.GuildAvailable(guildMock.Object);
        
        var guildModel = await Context.Guilds.FindAsync(guildId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.Name, Is.EqualTo(guildName));
        }
    }

    [Test]
    public async Task GuildNotInDatabase_ChannelsCreated()
    {
        const ulong guildId = 3UL;
        const string guildName = "new-guild";
        const ulong channel1Id = 1UL;
        const ulong channel2Id = 2UL;
        const string channel1Name = "new-channel1";
        const string channel2Name = "new-channel2";
        Mock<IGuild> guildMock = new();
        guildMock.SetupGet(g => g.Id).Returns(guildId);
        guildMock.SetupGet(g => g.Name).Returns(guildName);
        Mock<IGuildChannel> channelMock1 = new();
        Mock<IGuildChannel> channelMock2 = new();
        channelMock1.SetupGet(c => c.Id).Returns(channel1Id);
        channelMock1.SetupGet(c => c.Name).Returns(channel1Name);
        channelMock2.SetupGet(c => c.Id).Returns(channel2Id);
        channelMock2.SetupGet(c => c.Name).Returns(channel2Name);
        channelMock1
            .Setup(c => c.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>());
        channelMock2
            .Setup(c => c.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>());
        channelMock1.SetupGet(c => c.ChannelType)
                .Returns(ChannelType.Text);
        channelMock2.SetupGet(c => c.ChannelType)
            .Returns(ChannelType.Text);
        guildMock.Setup(g => g.GetChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync([channelMock1.Object, channelMock2.Object]);
        
        await SyncService.GuildAvailable(guildMock.Object);

        var channelModels = await Context.Channels
            .Where(c => c.GuildId == guildId)
            .OrderBy(c => c.Id)
            .ToListAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(channelModels, Has.Count.EqualTo(2));
            Assert.That(channelModels[0], Is.Not.Null);
            Assert.That(channelModels[1], Is.Not.Null);
            Assert.That(channelModels[0].Id, Is.EqualTo(channel1Id));
            Assert.That(channelModels[1].Id, Is.EqualTo(channel2Id));
            Assert.That(channelModels[0].Name, Is.EqualTo(channel1Name));
            Assert.That(channelModels[1].Name, Is.EqualTo(channel2Name));
        }
    }

    [Test]
    public async Task GuildNotInDatabase_UsersCreated()
    {
        const ulong guildId = 3UL;
        const string guildName = "new-guild";
        const ulong channel1Id = 1UL;
        const ulong channel2Id = 2UL;
        const string channel1Name = "new-channel1";
        const string channel2Name = "new-channel2";
        const ulong user1Id = 1UL;
        const  ulong user2Id = 2UL;
        const string user1Name = "test-user1";
        const string user2Name = "test-user2";
        Mock<IGuild> guildMock = new();
        guildMock.SetupGet(g => g.Id).Returns(guildId);
        guildMock.SetupGet(g => g.Name).Returns(guildName);
        Mock<IGuildChannel> channelMock1 = new();
        channelMock1.SetupGet(c => c.Id).Returns(channel1Id);
        channelMock1.SetupGet(c => c.Name).Returns(channel1Name);
        channelMock1.SetupGet(c => c.ChannelType).Returns(ChannelType.Text);
        Mock<IGuildChannel> channelMock2 = new();
        channelMock2.SetupGet(c => c.Id).Returns(channel2Id);
        channelMock2.SetupGet(c => c.Name).Returns(channel2Name);
        channelMock2.SetupGet(c => c.ChannelType).Returns(ChannelType.Text);
        Mock<IGuildUser> userMock1 = new();
        userMock1.SetupGet(u => u.Id).Returns(user1Id);
        userMock1.SetupGet(u => u.Username).Returns(user1Name);
        Mock<IGuildUser> userMock2 = new();
        userMock2.SetupGet(u => u.Id).Returns(user2Id);
        userMock2.SetupGet(u => u.Username).Returns(user2Name);
        List<IReadOnlyCollection<IGuildUser>> users1 =
        [
            new List<IGuildUser> { userMock1.Object, userMock2.Object }
        ];
        List<IReadOnlyCollection<IGuildUser>> users2 =
        [
            new List<IGuildUser> { userMock1.Object }
        ];
        channelMock1.Setup(c => c.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(users1.ToAsyncEnumerable());
        channelMock2.Setup(c => c.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(users2.ToAsyncEnumerable());
        guildMock.Setup(g => g.GetChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync([channelMock1.Object, channelMock2.Object]);
        
        await SyncService.GuildAvailable(guildMock.Object);

        var guildModel =  await Context.Guilds
            .AsSplitQuery()
            .Include(g => g.GuildUsers)
            .Include(g => g.Channels)
            .ThenInclude(c => c.ChannelUsers)
            .SingleOrDefaultAsync(g => g.Id == guildId);
        var userModel = await Context.Users.FindAsync(user2Id);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(guildModel, Is.Not.Null);
            Assert.That(guildModel!.GuildUsers, Has.Count.EqualTo(2));
            Assert.That(guildModel.Channels, Has.Count.EqualTo(2));
            Assert.That(guildModel.Channels.First(c => c.Id == channel1Id).ChannelUsers, Has.Count.EqualTo(2));
            Assert.That(userModel, Is.Not.Null);
            Assert.That(userModel!.Username, Is.EqualTo(user2Name));
        }
    }
}