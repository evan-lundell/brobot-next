using Brobot.Contexts;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brobot.Tests.HotOpServiceTests;

[TestFixture]
public abstract class HotOpServiceTestsBase
{
    protected BrobotDbContext Context;
    protected HotOpService HotOpService;
    protected IUnitOfWork UnitOfWork;
    
    [SetUp]
    public void Setup()
    {
        ServiceCollection serviceCollection = new();
        serviceCollection.AddDbContext<BrobotDbContext>(options => options.UseInMemoryDatabase("Brobot"));
        serviceCollection.AddTransient<IUnitOfWork, UnitOfWork>();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        Context = serviceProvider.GetRequiredService<BrobotDbContext>();

        SetupDatabase();

        Context.SaveChanges();
        UnitOfWork = new UnitOfWork(Context);
        HotOpService = new HotOpService(serviceProvider);
    }
    
    [TearDown]
    public void TearDown()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }

    protected virtual void SetupDatabase()
    {
        var guild = new GuildModel
        {
            Id = 1,
            Name = "Test Guild"
        };
        Context.Guilds.Add(guild);

        ChannelModel[] channels =
        [
            CreateChannel(1, guild),
            CreateChannel(2, guild)
        ];

        UserModel[] users =
        [
            CreateUser(1, guild, channels),
            CreateUser(2, guild, channels),
            CreateUser(3, guild, [channels[0]])
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
            User = users[0],
            UserId = users[0].Id,
            StartDate = DateTimeOffset.UtcNow.AddDays(-1),
            EndDate = DateTimeOffset.UtcNow.AddDays(1)
        };
        Context.HotOps.Add(hotOp1);
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
            User = users[1],
            UserId = users[1].Id,
            StartDate = DateTimeOffset.UtcNow.AddDays(-10),
            EndDate = DateTimeOffset.UtcNow.AddDays(-8)
        };
        Context.HotOps.Add(hotOp2);
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
            User = users[2],
            UserId = users[2].Id,
            StartDate = DateTimeOffset.UtcNow.AddDays(5),
            EndDate = DateTimeOffset.UtcNow.AddDays(10)
        };
        Context.HotOps.Add(hotOp3);
        channels[0].HotOps.Add(hotOp3);
        users[2].HotOps.Add(hotOp3);

        /*
         * HotOp 4
         * Owner: Test User 1
         * Channel: Channel 1
         * Status: Current
         * No sessions
         */
        var hotOp4 = new HotOpModel
        {
            Channel = channels[1],
            ChannelId = channels[1].Id,
            User = users[1],
            UserId = users[1].Id,
            StartDate = DateTimeOffset.UtcNow.AddHours(-1),
            EndDate = DateTimeOffset.UtcNow.AddDays(5)
        };
        Context.HotOps.Add(hotOp4);
        channels[1].HotOps.Add(hotOp4);
        users[1].HotOps.Add(hotOp4);
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
        Context.Channels.Add(channel);
        guild.Channels.Add(channel);
        return channel;
    }

    protected UserModel CreateUser(ulong id, GuildModel guild, ChannelModel[] channels, string timezone = "america/chicago")
    {
        var user = new UserModel
        {
            Id = id,
            Username = $"Test User {id}",
            Timezone = timezone
        };
        Context.Users.Add(user);
        var guildUser = new GuildUserModel
        {
            Guild = guild,
            GuildId = guild.Id,
            User = user,
            UserId = user.Id
        };
        guild.GuildUsers.Add(guildUser);
        user.GuildUsers.Add(guildUser);

        foreach (var channel in channels)
        {
            var channelUser = new ChannelUserModel
            {
                Channel = channel,
                ChannelId = channel.Id,
                User = user,
                UserId = user.Id
            };
            channel.ChannelUsers.Add(channelUser);
            user.ChannelUsers.Add(channelUser);
        }

        return user;
    }
    
    private void CreateHotOpSession(HotOpModel hotOpModel, UserModel user, DateTimeOffset startDateTime, int? lengthInMinutes)
    {
        var hotOpSession = new HotOpSessionModel
        {
            HotOp = hotOpModel,
            HotOpId = hotOpModel.Id,
            User = user,
            UserId = user.Id,
            StartDateTime = startDateTime,
            EndDateTime = lengthInMinutes.HasValue ? startDateTime.AddMinutes(lengthInMinutes.Value) : null
        };
        
        hotOpModel.HotOpSessions.Add(hotOpSession);
    }
}