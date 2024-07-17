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
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var wordCloudService = scope.ServiceProvider.GetRequiredService<WordCloudService>();
            var channels = await uow.Channels.GetAll();
            foreach (var channel in channels)
            {
                if (!channel.MonthlyWordCloud)
                {
                    continue;
                }

                var socketTextChannel = await _client.GetChannelAsync(channel.Id) as SocketTextChannel;
                if (socketTextChannel == null)
                {
                    continue;
                }
                var wordCloud = await wordCloudService.GenerateWordCloud(channel.Id);
                if (wordCloud.Length == 0)
                {
                    continue;
                }
                var fileName = "wordcloud.png";
                await File.WriteAllBytesAsync(fileName, wordCloud, cancellationToken);
                await socketTextChannel.SendFileAsync(fileName);
                File.Delete(fileName);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Word Cloud failed");
        }
    }
}