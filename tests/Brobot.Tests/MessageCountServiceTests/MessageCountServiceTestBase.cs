using AutoMapper;
using Brobot.Contexts;
using Brobot.Models;
using Brobot.Profiles;
using Brobot.Repositories;
using Brobot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using TimeZoneConverter;

namespace Brobot.Tests.MessageCountServiceTests;

[TestFixture]
public abstract class MessageCountServiceTestBase
{
    protected BrobotDbContext Context;
    protected MessageCountService MessageCountService;
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void SetUp()
    {
        ServiceCollection serviceCollection = new();
        serviceCollection.AddDbContext<BrobotDbContext>(options => options.UseLazyLoadingProxies().UseInMemoryDatabase("Brobot"));
        _serviceProvider = serviceCollection.BuildServiceProvider();
        Context = _serviceProvider.GetRequiredService<BrobotDbContext>();
        var testGuild = new GuildModel
        {
            Id = 1,
            Name = "Test Guild",
            Archived = false
        };
        var channel1 = CreateChannel(1, "Channel1", testGuild);
        var channel2 = CreateChannel(2, "Channel2", testGuild);

        var timezoneString = "america/chicago";
        var user1 = CreateUser(1, "User1", timezoneString, testGuild, testGuild.Channels);
        var user2 = CreateUser(2, "User2", timezoneString, testGuild, testGuild.Channels);
        var user3 = CreateUser(3, "User3", timezoneString, testGuild, [testGuild.Channels.First()]);
        var user4 = CreateUser(4, "User4", null, testGuild, [testGuild.Channels.First()]);
        var user5 = CreateUser(5, "User5", timezoneString, testGuild, [testGuild.Channels.First()]);

        var timezone = TZConvert.GetTimeZoneInfo(timezoneString);
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);
        var date = DateOnly.FromDateTime(DateTime.UtcNow + offset);
        CreateDailyMessageCount(user1, channel1, date, 10);
        CreateDailyMessageCount(user2, channel1, date, 15);
        CreateDailyMessageCount(user3, channel2, date, 5);
        CreateDailyMessageCount(user1, channel2, date, 2);
        CreateDailyMessageCount(user2, channel2, date, 3);

        date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1) + offset);
        CreateDailyMessageCount(user1, channel1, date, 5);
        CreateDailyMessageCount(user2, channel1, date, 4);
        CreateDailyMessageCount(user3, channel2, date, 3);
        CreateDailyMessageCount(user1, channel2, date, 2);
        CreateDailyMessageCount(user2, channel2, date, 1);
        CreateDailyMessageCount(user5, channel1, date, 1);

        date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2) + offset);
        CreateDailyMessageCount(user1, channel1, date, 50);
        CreateDailyMessageCount(user2, channel1, date, 40);
        CreateDailyMessageCount(user3, channel2, date, 30);
        CreateDailyMessageCount(user1, channel2, date, 20);
        CreateDailyMessageCount(user2, channel2, date, 10);

        Context.SaveChanges();

        var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<BrobotProfile>()));
        var unitOfWork = new UnitOfWork(Context);
        MessageCountService = new MessageCountService(unitOfWork, mapper);
    }
    
    private ChannelModel CreateChannel(ulong id, string name, GuildModel guild)
    {
        var channel = new ChannelModel
        {
            Id = id,
            Name = name,
            Archived = false,
            Guild = guild
        };
        Context.Channels.Add(channel);
        guild.Channels.Add(channel);
        return channel;
    }

    private UserModel CreateUser(ulong id, string username, string? timezone, GuildModel guild,
        IEnumerable<ChannelModel> channelModels)
    {
        var user = new UserModel
        {
            Id = id,
            Username = username,
            Timezone = timezone
        };
        var guildUser = new GuildUserModel
        {
            Guild = guild,
            GuildId = guild.Id,
            User = user,
            UserId = user.Id
        };
        Context.Users.Add(user);
        user.GuildUsers.Add(guildUser);
        guild.GuildUsers.Add(guildUser);

        foreach (var channel in channelModels)
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

    private void CreateDailyMessageCount(UserModel user, ChannelModel channelModel, DateOnly countDate,
        int messageCount)
    {
        var messageCounts = new DailyMessageCountModel
        {
            User = user,
            UserId = user.Id,
            Channel = channelModel,
            ChannelId = channelModel.Id,
            CountDate = countDate,
            MessageCount = messageCount
        };
        Context.DailyMessageCounts.Add(messageCounts);
        user.DailyCounts.Add(messageCounts);
        channelModel.DailyMessageCounts.Add(messageCounts);
    }
    
    [TearDown]
    public void TearDown()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        _serviceProvider.Dispose();
    }
}