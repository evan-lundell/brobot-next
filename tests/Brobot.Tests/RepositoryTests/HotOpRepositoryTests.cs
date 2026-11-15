using Brobot.Contexts;
using Brobot.Models;
using Brobot.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Tests.RepositoryTests;

[TestFixture]
public class HotOpRepositoryTests
{
    private const ulong OwnerId = 1;
    private const ulong OtherUserId = 2;
    private static readonly int[] OwnersUpcomingIds = [1];
    private static readonly int[] AllUpcomingIds = [1, 2];

    private DbContextOptions<BrobotDbContext> _options = null!;

    [SetUp]
    public void SetUp()
    {
        _options = new DbContextOptionsBuilder<BrobotDbContext>()
            .UseInMemoryDatabase($"Brobot_{Guid.NewGuid()}")
            .Options;

        using var context = new BrobotDbContext(_options);
        var guild = new GuildModel { Id = 1, Name = "Guild" };
        var channel = new ChannelModel { Id = 100, Name = "Channel", Guild = guild, GuildId = guild.Id };
        var owner = new DiscordUserModel { Id = OwnerId, Username = "Owner" };
        var other = new DiscordUserModel { Id = OtherUserId, Username = "Other" };
        var now = DateTimeOffset.UtcNow;
        context.HotOps.AddRange(
            HotOp(1, owner, channel, now.AddDays(1)),
            HotOp(2, other, channel, now.AddDays(2)),
            HotOp(3, owner, channel, now.AddDays(-1)), // already started
            HotOp(4, other, channel, now.AddDays(-2))); // already started
        context.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        using var context = new BrobotDbContext(_options);
        context.Database.EnsureDeleted();
    }

    [Test]
    public async Task GetUpcomingHotOps_WithOwner_ReturnsOnlyThatOwnersFutureHotOps()
    {
        var hotOps = await Query(OwnerId);

        Assert.That(hotOps.Select(ho => ho.Id), Is.EquivalentTo(OwnersUpcomingIds));
    }

    [Test]
    public async Task GetUpcomingHotOps_WithoutOwner_ReturnsEveryOwnersFutureHotOps()
    {
        var hotOps = await Query(null);

        Assert.That(hotOps.Select(ho => ho.Id), Is.EquivalentTo(AllUpcomingIds));
    }

    [Test]
    public async Task GetUpcomingHotOps_LoadsOwnerAndChannel()
    {
        // A fresh context, so the navigations can only come from the query's includes and not the change tracker
        var hotOp = (await Query(OwnerId)).Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(hotOp.DiscordUser, Is.Not.Null);
            Assert.That(hotOp.DiscordUser.Username, Is.EqualTo("Owner"));
            Assert.That(hotOp.Channel, Is.Not.Null);
            Assert.That(hotOp.Channel.Name, Is.EqualTo("Channel"));
        }
    }

    private async Task<List<HotOpModel>> Query(ulong? ownerId)
    {
        await using var context = new BrobotDbContext(_options);
        var repository = new HotOpRepository(context);
        return [.. await repository.GetUpcomingHotOps(ownerId)];
    }

    private static HotOpModel HotOp(int id, DiscordUserModel owner, ChannelModel channel, DateTimeOffset start) => new()
    {
        Id = id,
        UserId = owner.Id,
        DiscordUser = owner,
        ChannelId = channel.Id,
        Channel = channel,
        StartDate = start,
        EndDate = start.AddHours(2)
    };
}
