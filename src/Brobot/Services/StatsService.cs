using Brobot.Dtos;
using Brobot.Models;
using Brobot.Repositories;
using Discord;

namespace Brobot.Services;

public class StatsService(
    IUnitOfWork uow,
    IDiscordClient discordClient,
    IWordCountService wordCountService,
    IWordCloudService wordCloudService,
    ILogger<StatsService> logger) : IStatsService
{
    public async Task<StatsDto> GetStats(
        ChannelModel channel,
        DateOnly startDate,
        DateOnly endDate,
        int? statPeriodId = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting stats for channel {Channel}", channel.Id);
        if (endDate < startDate)
        {
            throw new InvalidOperationException("End date must be greater than start date");
        }
        var wordCountsTask = wordCountService.GetWordCount(channel, startDate.ToDateTime(TimeOnly.MinValue), endDate.ToDateTime(TimeOnly.MaxValue), cancellationToken);
        var messageCountsTask = uow.DailyMessageCounts.GetTotalDailyMessageCountsByChannel(startDate, endDate, channel.Id, cancellationToken);
        await Task.WhenAll(wordCountsTask, messageCountsTask);
        
        logger.LogInformation("Finished gathering word and message counts for {Channel}", channel.Id);
        StatsDto stats = new()
        {
            ChannelId = channel.Id,
            StartDate = startDate,
            EndDate = endDate,
            WordCounts = wordCountsTask.Result,
            MessageCounts = messageCountsTask.Result
                .GroupBy(mc => (UserId: mc.DiscordUserId, mc.DiscordUser.Username))
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
            logger.LogInformation("Stat Period found, updating database");
            var statPeriod = await uow.StatPeriods.GetById(statPeriodId.Value, cancellationToken);
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
                    statPeriod.UserMessageCounts.Add(new DiscordUserMessageCountModel
                    {
                        StatPeriod = statPeriod,
                        StatPeriodId = statPeriod.Id,
                        DiscordUserId = messageCount.UserId,
                        Count = messageCount.Count
                    });
                }
            }

            await uow.CompleteAsync(cancellationToken);
            logger.LogInformation("Finished updating database");
        }
        
        logger.LogInformation("Finished getting stats for channel {Channel}", channel.Id);
        return stats;
    }

    public async Task SendStats(ulong channelId, StatsDto stats, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending stats for channel {Channel}", channelId);
        cancellationToken.ThrowIfCancellationRequested();
        if (await discordClient.GetChannelAsync(channelId) is not ITextChannel textChannel)
        {
            logger.LogWarning("Unable to find text channel {Channel}", channelId);
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
            cancellationToken.ThrowIfCancellationRequested();
            await textChannel.SendMessageAsync(embed: embedBuilder.Build());
        }
        else
        {
            var wordCloudBytes = await wordCloudService.GetWordCloud(stats.WordCounts, cancellationToken);
            using var stream = new MemoryStream(wordCloudBytes);
            cancellationToken.ThrowIfCancellationRequested();
            await textChannel.SendFileAsync(stream: stream, filename: "wordcloud.png", embed: embedBuilder.Build());
        }
        logger.LogInformation("Sent stats for channel {Channel}", channelId);
    }
}