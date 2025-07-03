using Brobot.Contexts;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TimeZoneConverter;

namespace Brobot.Tests;

[TestFixture]
public class MessageCountServiceTests
{
    private BrobotDbContext _context = default!;
    private MessageCountService _messageCountService = default!;

    [SetUp]
    public void SetUp()
    {
        InMemoryDatabaseRoot root = new();
        var options = new DbContextOptionsBuilder<BrobotDbContext>()
            .UseInMemoryDatabase("TestDatabase", root)
            .Options;
        _context = new BrobotDbContext(options);
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

        date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2) + offset);
        CreateDailyMessageCount(user1, channel1, date, 50);
        CreateDailyMessageCount(user2, channel1, date, 40);
        CreateDailyMessageCount(user3, channel2, date, 30);
        CreateDailyMessageCount(user1, channel2, date, 20);
        CreateDailyMessageCount(user2, channel2, date, 10);

        _context.SaveChanges();

        var unitOfWork = new UnitOfWork(_context);
        _messageCountService = new MessageCountService(unitOfWork);
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
        _context.Channels.Add(channel);
        guild.Channels.Add(channel);
        return channel;
    }

    private UserModel CreateUser(ulong id, string username, string timezone, GuildModel guild,
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
        _context.Users.Add(user);
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
        _context.DailyMessageCounts.Add(messageCounts);
        user.DailyCounts.Add(messageCounts);
        channelModel.DailyMessageCounts.Add(messageCounts);
    }

    [Test]
    public async Task GetUsersDailyMessageCountForChannel_ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);

        var counts = (await _messageCountService.GetUsersDailyMessageCountForChannel(1, 1, 10)).ToList();
        var orderedCounts = counts.OrderBy(x => x.CountDate).ToList();
        var sum = counts.Sum(c => c.MessageCount);
        Assert.Multiple(() =>
        {
            Assert.That(counts.Count, Is.EqualTo(10));
            Assert.That(orderedCounts[9].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(orderedCounts[9].MessageCount, Is.EqualTo(10));
            Assert.That(orderedCounts[0].CountDate,
                Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-9) + offset)));
            Assert.That(orderedCounts[0].MessageCount, Is.EqualTo(0));
            Assert.That(sum, Is.EqualTo(65));
        });
    }

    [Test]
    public async Task GetUsersTotalDailyMessageCounts_ReturnsCorrectCounts()
    {
        var user = await _context.Users.FindAsync((ulong)1);
        if (user == null)
        {
            throw new Exception("User not defined");
        }

        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);

        var counts = (await _messageCountService.GetUsersTotalDailyMessageCounts(user, 10)).ToList();
        var orderedCounts = counts.OrderBy(c => c.CountDate).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(counts.Count, Is.EqualTo(10));
            Assert.That(orderedCounts[0].CountDate,
                Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-9) + offset)));
            Assert.That(orderedCounts[0].MessageCount, Is.EqualTo(0));
            Assert.That(orderedCounts[9].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(orderedCounts[9].MessageCount, Is.EqualTo(12));
            Assert.That(counts.Sum(c => c.MessageCount), Is.EqualTo(89));
        });
    }

    [Test]
    public async Task GetTopToday_ReturnsCorrectCounts()
    {
        var user2 = await _context.Users.FindAsync((ulong)2);
        if (user2 == null)
        {
            throw new Exception("User1 not defined");
        }

        var counts = (await _messageCountService.GetTopToday(user2)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(counts[0].User.Username, Is.EqualTo(user2.Username));
            Assert.That(counts[0].MessageCount, Is.EqualTo(18));
            Assert.That(counts[1].MessageCount, Is.EqualTo(12));
            Assert.That(counts[2].MessageCount, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task AddToDailyCount_AddsToExistingCount()
    {
        ulong userId = 1;
        ulong channelId = 1;
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var countDate = DateOnly.FromDateTime(DateTime.UtcNow + timezone.GetUtcOffset(DateTime.UtcNow));

        await _messageCountService.AddToDailyCount(userId, channelId, countDate);

        var counts = await _context.DailyMessageCounts
            .Where(dmc => dmc.UserId == userId && dmc.ChannelId == channelId && dmc.CountDate == countDate)
            .ToListAsync();
        Assert.That(counts.Count, Is.EqualTo(1));
        Assert.That(counts[0].MessageCount, Is.EqualTo(11));

    }

    [Test]
    public async Task AddToDailyCount_AddsNewCount()
    {
        ulong userId = 1;
        ulong channelId = 1;
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var countDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3) + timezone.GetUtcOffset(DateTime.UtcNow));

        await _messageCountService.AddToDailyCount(userId, channelId, countDate);

        var counts = await _context.DailyMessageCounts
            .Where(dmc => dmc.UserId == userId && dmc.ChannelId == channelId && dmc.CountDate == countDate)
            .ToListAsync();
        Assert.That(counts.Count, Is.EqualTo(1));
        Assert.That(counts[0].MessageCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetUsersTopDays_ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);
        var user = await _context.Users.FindAsync((ulong)1);
        if (user == null)
        {
            throw new Exception("User not defined");
        }

        var counts = (await _messageCountService.GetUsersTopDays(user, 10)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(counts[0].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2) + offset)));
            Assert.That(counts[0].MessageCount, Is.EqualTo(70));
            Assert.That(counts[1].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(counts[1].MessageCount, Is.EqualTo(12));
        });
    }

    [Test]
    public async Task GetUsersTopDaysInChannel_ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);
        var user = await _context.Users.FindAsync((ulong)1);
        if (user == null)
        {
            throw new Exception("User not defined");
        }

        var counts = (await _messageCountService.GetUsersTopDaysByChannel(user, 1, 10)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(counts[0].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2) + offset)));
            Assert.That(counts[0].MessageCount, Is.EqualTo(50));
            Assert.That(counts[1].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(counts[1].MessageCount, Is.EqualTo(10));
        });
    }

    [Test]
    public async Task GetTopTodayByChannel_ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);
        var user = await _context.Users.FindAsync((ulong)1);
        if (user == null)
        {
            throw new Exception("User not defined");
        }

        var counts = (await _messageCountService.GetTopTodayByChannel(user, 1)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(counts[0].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(counts[0].User.Username, Is.EqualTo("User1"));
            Assert.That(counts[0].MessageCount, Is.EqualTo(15));
        });
    }

    [Test]
    public async Task GetTotalDailyMessageCounts_ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);

        var counts = (await _messageCountService.GetTotalDailyMessageCounts(10, "america/chicago")).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(counts[0].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-9) + offset)));
            Assert.That(counts[0].MessageCount, Is.EqualTo(0));
            Assert.That(counts[9].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(counts[9].MessageCount, Is.EqualTo(35));

        });
    }

    [Test]
    public async Task GetTotalDailyMessageCountsByChannel_ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);

        var counts = (await _messageCountService.GetTotalDailyMessageCountsByChannel(10, 1, "america/chicago"))
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(counts[0].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-9) + offset)));
            Assert.That(counts[0].MessageCount, Is.EqualTo(0));
            Assert.That(counts[9].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(counts[9].MessageCount, Is.EqualTo(25));
        });
    }

    [Test]
    public async Task GetTotalTopDays_ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);

        var counts = (await _messageCountService.GetTotalTopDays(10)).ToList();
        
        Assert.Multiple(() =>
        {
            Assert.That(counts[0].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2) + offset)));
            Assert.That(counts[0].MessageCount, Is.EqualTo(150));
            Assert.That(counts[1].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(counts[1].MessageCount, Is.EqualTo(35));
        });
    }
    
    [Test]
    public async Task GetTotalTopDaysByChannel_ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);

        var counts = (await _messageCountService.GetTotalTopDaysByChannel(1, 10)).ToList();
        
        Assert.Multiple(() =>
        {
            Assert.That(counts[0].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2) + offset)));
            Assert.That(counts[0].MessageCount, Is.EqualTo(90));
            Assert.That(counts[1].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(counts[1].MessageCount, Is.EqualTo(25));
        });
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}