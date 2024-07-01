using Brobot.Repositories;
using Brobot.Services;
using Discord.WebSocket;

namespace Brobot.Workers;

public class WordCloudWorker : CronWorkerBase
{
    private readonly IServiceProvider _services;
    private readonly ILogger<WordCloudWorker> _logger;
    private readonly DiscordSocketClient _client;

    public WordCloudWorker(
        ICronWorkerConfig<WordCloudWorker> config,
        IServiceProvider services,
        ILogger<WordCloudWorker> logger,
        DiscordSocketClient client)
        : base(config.CronExpression)
    {
        _services = services;
        _logger = logger;
        _client = client;
    }

    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var wordCloudService = scope.ServiceProvider.GetRequiredService<WordCloudService>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var eligibleChannels = await uow.Channels.Find(c => c.Archived == false && c.MonthlyWordCloud);
            foreach (var channel in eligibleChannels)
            {
                var result = await wordCloudService.GenerateWordCloud(channel);
                if (result == null)
                {
                    continue;
                }

                var socketChannel = await _client.GetChannelAsync(channel.Id);
                if (socketChannel is not SocketTextChannel socketTextChannel)
                {
                    continue;
                }
             
                var fileName = "wordcloud.png";
                await File.WriteAllBytesAsync(fileName, result.Image, cancellationToken);
                var messageCountsMessage = string.Join("\n", result.UserMessageCounts.OrderByDescending(x => x.Value).Select(c => $"{c.Key}: {c.Value}"));
                await socketTextChannel.SendFileAsync(fileName, messageCountsMessage);
                File.Delete(fileName);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Word Cloud failed");
        }
    }
}