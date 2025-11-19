using Brobot.Dtos;
using Brobot.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Brobot.Tests.StatsServiceTests;

public class GetStatsTests : StatsServiceTestBase
{
    [Test]
    public void EndDateBeforeStartDate_ThrowsException()
    {
        GuildModel guild = new()
        {
            Id = 1UL,
            Name = "test-guild"
        };
        ChannelModel channelModel = new()
        {
            Id = 1UL,
            Name = "test-channel",
            GuildId = guild.Id,
            Guild = guild
        };
        
        Assert.ThrowsAsync<InvalidOperationException>(() => StatsService.GetStats(channelModel, new DateOnly(2025, 08, 01), new DateOnly(2025, 07, 01)));
    }

    [Test]
    public async Task ValidDates_ReturnsCorrectCounts()
    {
        var guildId = 1UL;
        var guildName = "test-guild";
        var channelId = 2UL;
        var channelName = "test-channel";
        var user1Id = 3UL;
        var user1Name = "test-user-1";
        var user2Id = 4UL;
        var user2Name = "test-user-2";
        var messageCount1 = 10;
        var messageCount2 = 5;
        var messageCount3 = 12;
        var word1 = "hello";
        var word1Count = 10;
        var word2 = "world";
        var word2Count = 5;
        
        GuildModel guild = new()
        {
            Id = guildId,
            Name = guildName
        };
        await Context.Guilds.AddAsync(guild);
        ChannelModel channelModel = new()
        {
            Id = channelId,
            Name = channelName,
            GuildId = guild.Id,
            Guild = guild
        };
        await Context.Channels.AddAsync(channelModel);
        UserModel user1Model = new()
        {
            Id = user1Id,
            Username = user1Name
        };
        UserModel user2Model = new()
        {
            Id = user2Id,
            Username = user2Name
        };
        await Context.Users.AddRangeAsync(user1Model, user2Model);
        await Context.DailyMessageCounts.AddRangeAsync(new DailyMessageCountModel
        {
            Channel = channelModel,
            ChannelId = channelId,
            CountDate = new DateOnly(2025, 8, 1),
            MessageCount = messageCount1,
            User = user1Model,
            UserId = user1Id,
        }, new DailyMessageCountModel
        {
            Channel = channelModel,
            ChannelId = channelId,
            CountDate = new DateOnly(2025, 8, 2),
            MessageCount = messageCount2,
            User = user1Model,
            UserId = user1Id
        }, new DailyMessageCountModel
        {
            Channel = channelModel,
            ChannelId = channelId,
            CountDate = new DateOnly(2025, 8, 1),
            MessageCount = messageCount3,
            User = user2Model,
            UserId = user2Id
        });
        await Context.SaveChangesAsync();
        
        var startDate = new DateOnly(2025, 8, 1);
        var endDate = new DateOnly(2025, 9, 1);
        
        WordCountServiceMock.Setup(wcs =>
                wcs.GetWordCount(channelModel, startDate.ToDateTime(TimeOnly.MinValue),
                    endDate.ToDateTime(TimeOnly.MaxValue)))
            .ReturnsAsync([
                new WordCountDto
                {
                    ChannelId = channelModel.Id,
                    Count = word1Count,
                    Word = word1
                },
                new  WordCountDto
                {
                    ChannelId = channelModel.Id,
                    Count = word2Count,
                    Word = word2
                }
            ]);
        
        var stats = await StatsService.GetStats(channelModel, startDate, endDate);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(stats.ChannelId, Is.EqualTo(channelModel.Id));
            Assert.That(stats.StartDate, Is.EqualTo(startDate));
            Assert.That(stats.EndDate, Is.EqualTo(endDate));
            Assert.That(stats.MessageCounts.Count, Is.EqualTo(2));
            Assert.That(stats.MessageCounts.First().UserId, Is.EqualTo(user1Id));
            Assert.That(stats.MessageCounts.First().Count, Is.EqualTo(messageCount1 + messageCount2));
            Assert.That(stats.MessageCounts.Last().UserId, Is.EqualTo(user2Id));
            Assert.That(stats.MessageCounts.Last().Count, Is.EqualTo(messageCount3));
            Assert.That(stats.WordCounts.Count, Is.EqualTo(2));
            Assert.That(stats.WordCounts.First().Word, Is.EqualTo(word1));
            Assert.That(stats.WordCounts.First().Count, Is.EqualTo(word1Count));
            Assert.That(stats.WordCounts.Last().Word, Is.EqualTo(word2));
            Assert.That(stats.WordCounts.Last().Count, Is.EqualTo(word2Count));
        }
    }

    [Test]
    public async Task StatPeriodIncluded_UpdatesDatabase()
    {
        var guildId = 1UL;
        var guildName = "test-guild";
        var channelId = 2UL;
        var channelName = "test-channel";
        var user1Id = 3UL;
        var user1Name = "test-user-1";
        var user2Id = 4UL;
        var user2Name = "test-user-2";
        var messageCount1 = 10;
        var messageCount2 = 5;
        var messageCount3 = 12;
        var word1 = "hello";
        var word1Count = 10;
        var word2 = "world";
        var word2Count = 5;
        var statPeriodId = 1;
        
        GuildModel guild = new()
        {
            Id = guildId,
            Name = guildName
        };
        await Context.Guilds.AddAsync(guild);
        ChannelModel channelModel = new()
        {
            Id = channelId,
            Name = channelName,
            GuildId = guild.Id,
            Guild = guild
        };
        await Context.Channels.AddAsync(channelModel);
        UserModel user1Model = new()
        {
            Id = user1Id,
            Username = user1Name
        };
        UserModel user2Model = new()
        {
            Id = user2Id,
            Username = user2Name
        };
        await Context.Users.AddRangeAsync(user1Model, user2Model);
        await Context.DailyMessageCounts.AddRangeAsync(new DailyMessageCountModel
        {
            Channel = channelModel,
            ChannelId = channelId,
            CountDate = new DateOnly(2025, 8, 1),
            MessageCount = messageCount1,
            User = user1Model,
            UserId = user1Id,
        }, new DailyMessageCountModel
        {
            Channel = channelModel,
            ChannelId = channelId,
            CountDate = new DateOnly(2025, 8, 2),
            MessageCount = messageCount2,
            User = user1Model,
            UserId = user1Id
        }, new DailyMessageCountModel
        {
            Channel = channelModel,
            ChannelId = channelId,
            CountDate = new DateOnly(2025, 8, 1),
            MessageCount = messageCount3,
            User = user2Model,
            UserId = user2Id
        });
        var startDate = new DateOnly(2025, 8, 1);
        var endDate = new DateOnly(2025, 9, 1);
        await Context.StatPeriods.AddAsync(new StatPeriodModel
        {
            Id = 1,
            StartDate = startDate,
            EndDate = endDate,
            Channel = channelModel,
            ChannelId = channelId
        });
        await Context.SaveChangesAsync();
        
        
        
        WordCountServiceMock.Setup(wcs =>
                wcs.GetWordCount(channelModel, startDate.ToDateTime(TimeOnly.MinValue),
                    endDate.ToDateTime(TimeOnly.MaxValue)))
            .ReturnsAsync([
                new WordCountDto
                {
                    ChannelId = channelModel.Id,
                    Count = word1Count,
                    Word = word1
                },
                new  WordCountDto
                {
                    ChannelId = channelModel.Id,
                    Count = word2Count,
                    Word = word2
                }
            ]);
        
        await StatsService.GetStats(channelModel, startDate, endDate, statPeriodId);

        var updatedStatPeriod = await Context.StatPeriods
            .AsSplitQuery()
            .Include(sp => sp.UserMessageCounts)
            .Include(sp => sp.WordCounts)
            .SingleOrDefaultAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(updatedStatPeriod, Is.Not.Null);
            Assert.That(updatedStatPeriod!.ChannelId, Is.EqualTo(channelModel.Id));
            Assert.That(updatedStatPeriod.StartDate, Is.EqualTo(startDate));
            Assert.That(updatedStatPeriod.EndDate, Is.EqualTo(endDate));
            Assert.That(updatedStatPeriod.WordCounts.Count, Is.EqualTo(2));
            Assert.That(updatedStatPeriod.UserMessageCounts.Count, Is.EqualTo(2));
        }
    }

}