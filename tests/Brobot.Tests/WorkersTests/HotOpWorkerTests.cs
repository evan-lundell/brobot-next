using Brobot.Contexts;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Workers;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Brobot.Tests.WorkersTests;

/*
 * End-to-end tests for HotOpWorker.DoWork: real in-memory database, real UnitOfWork and HotOpService,
 * mocked Discord client. DoWork is called directly, so no clock or scheduling is involved.
 *
 * Seed data:
 * - Guild 1, text channel 100
 * - Users 1 ("Owner"), 2, 3, all members of channel 100
 * - Discord voice channel 500
 */
[TestFixture]
public class HotOpWorkerTests
{
    private const ulong GuildId = 1;
    private const ulong TextChannelId = 100;
    private const ulong VoiceChannelId = 500;
    private const ulong OwnerId = 1;
    private const string OwnerName = "Owner";
    private static readonly DateTimeOffset Now = new(2026, 12, 1, 20, 0, 0, TimeSpan.Zero);

    private ServiceProvider _provider = null!;
    private Mock<IDiscordClient> _clientMock = null!;
    private Mock<IGuild> _guildMock = null!;
    private Mock<IVoiceChannel> _voiceChannelMock = null!;
    private Mock<ISocketMessageChannel> _textChannelMock = null!;
    private TestableHotOpWorker _worker = null!;

    [SetUp]
    public async Task SetUp()
    {
        var services = new ServiceCollection();
        var dbName = $"Brobot_{Guid.NewGuid()}";
        services.AddDbContext<BrobotDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IHotOpService, HotOpService>();
        services.AddLogging();
        _provider = services.BuildServiceProvider();

        await SeedBaseData();

        _voiceChannelMock = new Mock<IVoiceChannel>();
        _voiceChannelMock.SetupGet(c => c.Id).Returns(VoiceChannelId);

        _guildMock = new Mock<IGuild>();
        _textChannelMock = new Mock<ISocketMessageChannel>();

        _clientMock = new Mock<IDiscordClient>();
        _clientMock
            .Setup(c => c.GetGuildAsync(GuildId, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync(_guildMock.Object);
        _clientMock
            .Setup(c => c.GetChannelAsync(TextChannelId, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync(_textChannelMock.Object);

        _worker = new TestableHotOpWorker(_provider.GetRequiredService<IServiceScopeFactory>(), _clientMock.Object);
    }

    [TearDown]
    public async Task TearDown()
    {
        _worker.Dispose();
        await _provider.DisposeAsync();
    }

    [Test]
    public async Task StartingHotOp_OwnerInVoice_CreatesSessionsAtStartDate_AndAnnounces()
    {
        var hotOpId = await AddHotOp(start: Now, end: Now.AddHours(2));
        // 999 is in voice but not in the database (e.g. a bot)
        SetUsersInVoice(OwnerId, 2, 3, 999);

        await _worker.Run(Now);

        var hotOp = await LoadHotOp(hotOpId);
        Assert.Multiple(() =>
        {
            Assert.That(hotOp.HotOpSessions.Select(s => s.DiscordUserId), Is.EquivalentTo(new[] { 2UL, 3UL }));
            Assert.That(hotOp.HotOpSessions.All(s => s.StartDateTime == Now && s.EndDateTime == null), Is.True);
            Assert.That(SentMessages(), Has.Member($"Operation Hot {OwnerName} has begun!"));
        });
    }

    [Test]
    public async Task StartingHotOp_OwnerNotInVoice_CreatesNoSessions_ButStillAnnounces()
    {
        var hotOpId = await AddHotOp(start: Now, end: Now.AddHours(2));
        SetOwner(voiceChannel: null);

        await _worker.Run(Now);

        var hotOp = await LoadHotOp(hotOpId);
        Assert.Multiple(() =>
        {
            Assert.That(hotOp.HotOpSessions, Is.Empty);
            Assert.That(SentMessages(), Has.Member($"Operation Hot {OwnerName} has begun!"));
        });
    }

    [Test]
    public async Task StartingHotOp_GuildNotFound_CreatesNoSessions_ButStillAnnounces()
    {
        var hotOpId = await AddHotOp(start: Now, end: Now.AddHours(2));
        _clientMock
            .Setup(c => c.GetGuildAsync(GuildId, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync((IGuild)null!);

        await _worker.Run(Now);

        var hotOp = await LoadHotOp(hotOpId);
        Assert.Multiple(() =>
        {
            Assert.That(hotOp.HotOpSessions, Is.Empty);
            Assert.That(SentMessages(), Has.Member($"Operation Hot {OwnerName} has begun!"));
        });
    }

    [Test]
    public async Task StartingHotOp_OwnerNotInGuild_CreatesNoSessions_ButStillAnnounces()
    {
        var hotOpId = await AddHotOp(start: Now, end: Now.AddHours(2));
        _guildMock
            .Setup(g => g.GetUserAsync(OwnerId, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync((IGuildUser)null!);

        await _worker.Run(Now);

        var hotOp = await LoadHotOp(hotOpId);
        Assert.Multiple(() =>
        {
            Assert.That(hotOp.HotOpSessions, Is.Empty);
            Assert.That(SentMessages(), Has.Member($"Operation Hot {OwnerName} has begun!"));
        });
    }

    [Test]
    public async Task StartingHotOp_GuildLookupThrows_CreatesNoSessions_ButStillAnnounces()
    {
        // e.g. a Discord outage: the lookup throws rather than returning null
        var hotOpId = await AddHotOp(start: Now, end: Now.AddHours(2));
        _clientMock
            .Setup(c => c.GetGuildAsync(GuildId, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ThrowsAsync(new InvalidOperationException("Discord is down"));

        await _worker.Run(Now);

        var hotOp = await LoadHotOp(hotOpId);
        Assert.Multiple(() =>
        {
            Assert.That(hotOp.HotOpSessions, Is.Empty);
            Assert.That(SentMessages(), Has.Member($"Operation Hot {OwnerName} has begun!"));
        });
    }

    [Test]
    public async Task EndingHotOp_ClosesOpenSessionsAtEndDate_LeavesClosedSessions_AndAnnounces()
    {
        var closedEnd = Now.AddMinutes(-80);
        var hotOpId = await AddHotOp(start: Now.AddHours(-2), end: Now, sessions:
        [
            (2UL, Now.AddMinutes(-90), null),
            (3UL, Now.AddMinutes(-100), closedEnd),
            (3UL, Now.AddMinutes(-30), null)
        ]);

        await _worker.Run(Now);

        var hotOp = await LoadHotOp(hotOpId);
        var closedSession = hotOp.HotOpSessions.Single(s => s.StartDateTime == Now.AddMinutes(-100));
        Assert.Multiple(() =>
        {
            Assert.That(hotOp.HotOpSessions.Any(s => s.EndDateTime == null), Is.False);
            Assert.That(hotOp.HotOpSessions.Count(s => s.EndDateTime == Now), Is.EqualTo(2));
            Assert.That(closedSession.EndDateTime, Is.EqualTo(closedEnd));
            Assert.That(SentMessages(), Has.Member($"Operation Hot {OwnerName} has ended!"));
        });
    }

    [Test]
    public async Task HotOpsNotStartingOrEndingThisMinute_AreIgnored()
    {
        var hotOpId = await AddHotOp(start: Now.AddMinutes(-1), end: Now.AddMinutes(1), sessions:
        [
            (2UL, Now.AddMinutes(-1), null)
        ]);
        SetUsersInVoice(OwnerId, 2, 3);

        await _worker.Run(Now);

        var hotOp = await LoadHotOp(hotOpId);
        Assert.Multiple(() =>
        {
            Assert.That(hotOp.HotOpSessions, Has.Count.EqualTo(1));
            Assert.That(hotOp.HotOpSessions.Single().EndDateTime, Is.Null);
            Assert.That(SentMessages(), Is.Empty);
        });
    }

    private async Task SeedBaseData()
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrobotDbContext>();

        var guild = new GuildModel { Id = GuildId, Name = "Guild" };
        var channel = new ChannelModel { Id = TextChannelId, Name = "Channel", Guild = guild, GuildId = GuildId };
        context.Guilds.Add(guild);
        context.Channels.Add(channel);

        foreach (var (id, name) in new[] { (OwnerId, OwnerName), (2UL, "User 2"), (3UL, "User 3") })
        {
            var user = new DiscordUserModel { Id = id, Username = name };
            context.DiscordUsers.Add(user);
            channel.ChannelUsers.Add(new ChannelDiscordUserModel
            {
                Channel = channel,
                ChannelId = channel.Id,
                DiscordUser = user,
                UserId = user.Id
            });
        }

        await context.SaveChangesAsync();
    }

    private async Task<int> AddHotOp(
        DateTimeOffset start,
        DateTimeOffset end,
        (ulong UserId, DateTimeOffset Start, DateTimeOffset? End)[]? sessions = null)
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrobotDbContext>();
        var owner = await context.DiscordUsers.SingleAsync(u => u.Id == OwnerId);
        var channel = await context.Channels.SingleAsync(c => c.Id == TextChannelId);

        var hotOp = new HotOpModel
        {
            UserId = OwnerId,
            DiscordUser = owner,
            ChannelId = TextChannelId,
            Channel = channel,
            StartDate = start,
            EndDate = end
        };
        foreach (var (userId, sessionStart, sessionEnd) in sessions ?? [])
        {
            var user = await context.DiscordUsers.SingleAsync(u => u.Id == userId);
            hotOp.HotOpSessions.Add(new HotOpSessionModel
            {
                HotOp = hotOp,
                DiscordUser = user,
                DiscordUserId = userId,
                StartDateTime = sessionStart,
                EndDateTime = sessionEnd
            });
        }

        context.HotOps.Add(hotOp);
        await context.SaveChangesAsync();
        return hotOp.Id;
    }

    private async Task<HotOpModel> LoadHotOp(int id)
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrobotDbContext>();
        return await context.HotOps
            .AsNoTracking()
            .Include(ho => ho.HotOpSessions)
            .SingleAsync(ho => ho.Id == id);
    }

    /// <summary>
    /// Puts the given users in voice channel 500. The owner (if included) is what the worker looks up via the guild;
    /// everyone is returned from the voice channel's user list. Each user's VoiceChannel must point at the same
    /// channel, because Extensions.GetConnectedUsersAsync filters on it.
    /// </summary>
    private void SetUsersInVoice(params ulong[] userIds)
    {
        var users = userIds.Select(id => GuildUser(id, _voiceChannelMock.Object)).ToList();
        if (userIds.Contains(OwnerId))
        {
            _guildMock
                .Setup(g => g.GetUserAsync(OwnerId, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(users.Single(u => u.Id == OwnerId));
        }

        _voiceChannelMock
            .Setup(c => c.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(SinglePage<IReadOnlyCollection<IGuildUser>>(users));
    }

    private void SetOwner(IVoiceChannel? voiceChannel)
    {
        _guildMock
            .Setup(g => g.GetUserAsync(OwnerId, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .ReturnsAsync(GuildUser(OwnerId, voiceChannel));
    }

    private static IGuildUser GuildUser(ulong id, IVoiceChannel? voiceChannel)
    {
        var user = new Mock<IGuildUser>();
        user.SetupGet(u => u.Id).Returns(id);
        user.SetupGet(u => u.VoiceChannel).Returns(voiceChannel!);
        return user.Object;
    }

    private static async IAsyncEnumerable<T> SinglePage<T>(T page)
    {
        yield return page;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Reads SendMessageAsync calls from the mock's invocation list instead of Setup/Verify, because Moq
    /// expression trees can't omit optional parameters and SendMessageAsync has a lot of them.
    /// </summary>
    private List<string?> SentMessages() => _textChannelMock.Invocations
        .Where(i => i.Method.Name == nameof(ISocketMessageChannel.SendMessageAsync))
        .Select(i => i.Arguments[0] as string)
        .ToList();

    private sealed class TestableHotOpWorker(IServiceScopeFactory scopeFactory, IDiscordClient client)
        : HotOpWorker(
            new CronWorkerConfig<HotOpWorker> { CronExpression = "* * * * *" },
            scopeFactory,
            client,
            NullLogger<HotOpWorker>.Instance,
            TimeProvider.System)
    {
        public Task Run(DateTimeOffset target) => DoWork(target, CancellationToken.None);
    }
}
