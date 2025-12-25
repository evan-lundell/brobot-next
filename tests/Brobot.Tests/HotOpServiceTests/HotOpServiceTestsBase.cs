using Brobot.Contexts;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brobot.Tests.HotOpServiceTests;

[TestFixture]
public abstract class HotOpServiceTestsBase
{
    protected IUnitOfWork UnitOfWork;
    protected HotOpService HotOpService;

    private BrobotDbContext _context;
    private ServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        ServiceCollection serviceCollection = new();
        var uniqueDbName = $"Brobot_{Guid.NewGuid()}";
        serviceCollection.AddDbContext<BrobotDbContext>(options => options.UseInMemoryDatabase(uniqueDbName));
        _serviceProvider = serviceCollection.BuildServiceProvider();

        _context = _serviceProvider.GetRequiredService<BrobotDbContext>();
        SetupDatabase();

        _context.SaveChanges();
        UnitOfWork = new UnitOfWork(_context);
        HotOpService = new HotOpService(UnitOfWork, Mock.Of<ILogger<HotOpService>>());
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _serviceProvider.Dispose();
        UnitOfWork.Dispose();
    }

    protected virtual void SetupDatabase()
    {
        var guild = new GuildModel
        {
            Id = 1,
            Name = "Test Guild"
        };
        _context.Guilds.Add(guild);

        ChannelModel[] channels =
        [
            CreateChannel(1, guild),
            CreateChannel(2, guild),
            CreateChannel(3, guild)
        ];

        DiscordUserModel[] users =
        [
            CreateUser(1, guild, channels),
            CreateUser(2, guild, channels),
            CreateUser(3, guild, [channels[0]]),
            CreateUser(4, guild, [channels[2]]),
            CreateUser(5, guild, [channels[2]])
        ];

        /*
         * HotOp 1
         * Owner: Test User 1
         * Channel: Channel 1
         * Status: Current
         * - User 2: 100 minutes
         * - User 3: 40 minutes
         */
        var hotOp1 = new HotOpModel
        {
            Channel = channels[0],
            ChannelId = channels[0].Id,
            DiscordUser = users[0],
            UserId = users[0].Id,
            StartDate = DateTimeOffset.UtcNow.AddDays(-1),
            EndDate = DateTimeOffset.UtcNow.AddDays(1)
        };
        _context.HotOps.Add(hotOp1);
        channels[0].HotOps.Add(hotOp1);
        users[0].HotOps.Add(hotOp1);
        CreateHotOpSession(hotOp1, users[1], DateTimeOffset.UtcNow.AddDays(-1), 60);
        CreateHotOpSession(hotOp1, users[2], DateTimeOffset.UtcNow.AddDays(-1).AddMinutes(10), 30);
        CreateHotOpSession(hotOp1, users[1], DateTimeOffset.UtcNow.AddHours(-5), 40);
        CreateHotOpSession(hotOp1, users[2], DateTimeOffset.UtcNow.AddMinutes(-10), null);
        
        /*
         * HotOp 2
         * Owner: Test User 2
         * Channel: Channel 2
         * Status: Past
         * - User 1: 120 minutes
         */
        var hotOp2 = new HotOpModel
        {
            Channel = channels[1],
            ChannelId = channels[1].Id,
            DiscordUser = users[1],
            UserId = users[1].Id,
            StartDate = DateTimeOffset.UtcNow.AddDays(-10),
            EndDate = DateTimeOffset.UtcNow.AddDays(-8)
        };
        _context.HotOps.Add(hotOp2);
        channels[1].HotOps.Add(hotOp2);
        users[1].HotOps.Add(hotOp2);
        CreateHotOpSession(hotOp2, users[0], DateTimeOffset.UtcNow.AddDays(-10).AddHours(5), 120);
        
        /*
         * HotOp 3
         * Owner: Test User 3
         * Channel: Channel 1
         * Status: Future
         */
        var hotOp3 = new HotOpModel
        {
            Channel = channels[0],
            ChannelId = channels[0].Id,
            DiscordUser = users[2],
            UserId = users[2].Id,
            StartDate = DateTimeOffset.UtcNow.AddDays(5),
            EndDate = DateTimeOffset.UtcNow.AddDays(10)
        };
        _context.HotOps.Add(hotOp3);
        channels[0].HotOps.Add(hotOp3);
        users[2].HotOps.Add(hotOp3);

        /*
         * HotOp 4
         * Owner: Test User 2
         * Channel: Channel 2
         * Status: Current
         * No sessions
         */
        var hotOp4 = new HotOpModel
        {
            Channel = channels[1],
            ChannelId = channels[1].Id,
            DiscordUser = users[1],
            UserId = users[1].Id,
            StartDate = DateTimeOffset.UtcNow.AddHours(-1),
            EndDate = DateTimeOffset.UtcNow.AddDays(5)
        };
        _context.HotOps.Add(hotOp4);
        channels[1].HotOps.Add(hotOp4);
        users[1].HotOps.Add(hotOp4);
        
        /*
         * HotOp 5
         * Owner: Test User 1
         * Channel: Channel 2
         * Status: Current
         * - User 2: 100 minutes
         * - User 3: 40 minutes
         */
        var hotOp5 = new HotOpModel
        {
            Channel = channels[1],
            ChannelId = channels[1].Id,
            DiscordUser = users[0],
            UserId = users[0].Id,
            StartDate = DateTimeOffset.UtcNow.AddDays(-1),
            EndDate = DateTimeOffset.UtcNow.AddDays(1)
        };
        _context.HotOps.Add(hotOp5);
        channels[1].HotOps.Add(hotOp5);
        users[0].HotOps.Add(hotOp5);
        CreateHotOpSession(hotOp5, users[1], DateTimeOffset.UtcNow.AddHours(-23), 100);
        CreateHotOpSession(hotOp5, users[2], DateTimeOffset.UtcNow.AddHours(-20).AddMinutes(10), 40);
    }

    protected ChannelModel CreateChannel(ulong id, GuildModel guild)
    {
        var channel = new ChannelModel
        {
            Id = id,
            Name = $"Test Channel {id}",
            Guild = guild,
            GuildId = guild.Id
        };
        _context.Channels.Add(channel);
        guild.Channels.Add(channel);
        return channel;
    }

    protected DiscordUserModel CreateUser(ulong id, GuildModel guild, ChannelModel[] channels, string timezone = "america/chicago")
    {
        var user = new DiscordUserModel
        {
            Id = id,
            Username = $"Test User {id}",
            Timezone = timezone
        };
        _context.DiscordUsers.Add(user);
        var guildUser = new GuildDiscordUserModel
        {
            Guild = guild,
            GuildId = guild.Id,
            DiscordUser = user,
            DiscordUserId = user.Id
        };
        guild.GuildDiscordUsers.Add(guildUser);
        user.GuildUsers.Add(guildUser);

        foreach (var channel in channels)
        {
            var channelUser = new ChannelDiscordUserModel
            {
                Channel = channel,
                ChannelId = channel.Id,
                DiscordUser = user,
                UserId = user.Id
            };
            channel.ChannelUsers.Add(channelUser);
            user.ChannelUsers.Add(channelUser);
        }

        return user;
    }
    
    private void CreateHotOpSession(HotOpModel hotOpModel, DiscordUserModel discordUser, DateTimeOffset startDateTime, int? lengthInMinutes)
    {
        var hotOpSession = new HotOpSessionModel
        {
            HotOp = hotOpModel,
            HotOpId = hotOpModel.Id,
            DiscordUser = discordUser,
            DiscordUserId = discordUser.Id,
            StartDateTime = startDateTime,
            EndDateTime = lengthInMinutes.HasValue ? startDateTime.AddMinutes(lengthInMinutes.Value) : null
        };
        
        hotOpModel.HotOpSessions.Add(hotOpSession);
    }

    protected async Task<HotOpModel> GetHotOp(int id)
    {
        return await _context.HotOps
            .Include(ho => ho.DiscordUser)
            .Include(ho => ho.HotOpSessions)
            .ThenInclude(hos => hos.DiscordUser)
            .Include(ho => ho.Channel)
            .ThenInclude(c => c.ChannelUsers)
            .ThenInclude(cu => cu.DiscordUser)
            .SingleAsync(ho => ho.Id == 5);
    }
}