using Brobot.Dtos;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Discord.WebSocket;

namespace Brobot.Workers;

public class MonthlyStatsWorker(
    ICronWorkerConfig<MonthlyStatsWorker> config,
    IServiceProvider provider,
    IWordCloudService wordCloudService,
    DiscordSocketClient discordClient) : CronWorkerBase(config.CronExpression)
{
    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        using var scope =  provider.CreateScope();
        using var iow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var wordCountService = scope.ServiceProvider.GetRequiredService<WordCountService>();
        var channels = await iow.Channels.Find(c => c.MonthlyWordCloud);
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = now.AddMonths(-1);
        var endDate = now.AddDays(-1);
        foreach (var channel in channels)
        {
            var messageCountsTask = iow.DailyMessageCounts.GetTotalDailyMessageCountsByChannel(startDate, endDate, channel.Id);
            var wordCountsTask = wordCountService.GetWordCount(channel, startDate.ToDateTime(TimeOnly.MinValue), endDate.ToDateTime(TimeOnly.MaxValue)); 
            await Task.WhenAll(messageCountsTask, wordCountsTask);
            await SendMessage(channel.Id, messageCountsTask.Result, wordCountsTask.Result);
        }
    }

    private async Task SendMessage(ulong channelId, IEnumerable<DailyMessageCountModel> messageCounts,
        IEnumerable<WordCountDto> wordCounts)
    {
        var dailyMessageCountModels = messageCounts.ToArray();
        var wordCountDtos = wordCounts.ToArray();
        if (dailyMessageCountModels.Length == 0 && wordCountDtos.Length == 0)
        {
            return;
        }

        var leaderboard = dailyMessageCountModels
            .GroupBy(mc => mc.User.Username)
            .Select(g => new { Username = g.Key, TotalCount = g.Sum(mc => mc.MessageCount) })
            .OrderByDescending(mc => mc.TotalCount)
            .Select((mc, i) => new EmbedFieldBuilder
            {
                Name = $"{i + 1}. {mc.Username}",
                Value = $"Messages: {mc.TotalCount}",
                IsInline = false
            })
            .ToList();

        var embedBuilder = new EmbedBuilder
        {
            Title = "Monthly Leaderboard",
            Fields = leaderboard
        };

        if (await discordClient.GetChannelAsync(channelId) is not SocketTextChannel channel)
        {
            return;
        }
        
        if (wordCountDtos.Length == 0)
        {
            await channel.SendMessageAsync(embed: embedBuilder.Build());
        }
        else
        {
            var wordCloudBytes = await wordCloudService.GetWordCloud(wordCountDtos);
            var stream = new MemoryStream(wordCloudBytes);
            await channel.SendFileAsync(stream: stream, filename: "wordcloud.png", embed: embedBuilder.Build());
        }
    }
}