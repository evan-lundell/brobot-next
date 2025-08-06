using Brobot.Dtos;
using Brobot.Models;
using Brobot.Repositories;
using Discord;

namespace Brobot.Services;

public class StatsService(
    IUnitOfWork uow,
    IDiscordClient discordClient,
    IWordCountService wordCountService,
    IWordCloudService wordCloudService) : IStatsService
{
    public async Task<StatsDto> GetStats(ChannelModel channel, DateOnly startDate, DateOnly endDate, int? statPeriodId = null)
    {
        if (endDate < startDate)
        {
            throw new Exception("End date must be greater than start date");
        }
        var wordCountsTask = wordCountService.GetWordCount(channel, startDate.ToDateTime(TimeOnly.MinValue), endDate.ToDateTime(TimeOnly.MaxValue));
        var messageCountsTask = uow.DailyMessageCounts.GetTotalDailyMessageCountsByChannel(startDate, endDate, channel.Id);
        await Task.WhenAll(wordCountsTask, messageCountsTask);
        
        StatsDto stats = new()
        {
            ChannelId = channel.Id,
            StartDate = startDate,
            EndDate = endDate,
            WordCounts = wordCountsTask.Result,
            MessageCounts = messageCountsTask.Result
                .GroupBy(mc => (mc.UserId, mc.User.Username))
                .Select(g => new MessageCountDto
                {
                    UserId = g.Key.UserId,
                    Username = g.Key.Username,
                    Count = g.Sum(mc => mc.MessageCount)
                })
                .OrderByDescending(mc => mc.Count)
        };
        if (statPeriodId.HasValue)
        {
            var statPeriod = await uow.StatPeriods.GetById(statPeriodId.Value);
            if (statPeriod != null)
            {
                foreach (var wordCount in stats.WordCounts.OrderByDescending(wc => wc.Count))
                {
                    statPeriod.WordCounts.Add(new WordCountModel
                    {
                        StatPeriod = statPeriod,
                        StatPeriodId = statPeriod.Id,
                        Word = wordCount.Word,
                        Count = wordCount.Count,
                    });
                }

                foreach (var messageCount in stats.MessageCounts.OrderByDescending(wc => wc.Count))
                {
                    statPeriod.UserMessageCounts.Add(new UserMessageCountModel
                    {
                        StatPeriod = statPeriod,
                        StatPeriodId = statPeriod.Id,
                        UserId = messageCount.UserId,
                        Count = messageCount.Count
                    });
                }
            }

            await uow.CompleteAsync();
        }
        
        return stats;
    }

    public async Task SendStats(ulong channelId, StatsDto stats)
    {
        if (await discordClient.GetChannelAsync(channelId) is not ITextChannel textChannel)
        {
            return;
        }

        var leaderboardEmbed = stats.MessageCounts
            .Select((mc, i) => new EmbedFieldBuilder
            {
                Name = $"{i + 1}. {mc.Username}",
                Value = $"Messages: {mc.Count}",
                IsInline = false
            })
            .ToList();
        EmbedBuilder embedBuilder = new()
        {
            Title = "Leaderboard",
            Fields = leaderboardEmbed
        };
        
        if (!stats.WordCounts.Any())
        {
            await textChannel.SendMessageAsync(embed: embedBuilder.Build());
        }
        else
        {
            var wordCloudBytes = await wordCloudService.GetWordCloud(stats.WordCounts);
            using var stream = new MemoryStream(wordCloudBytes);
            await textChannel.SendFileAsync(stream: stream, filename: "wordcloud.png", embed: embedBuilder.Build());
        }
    }
}