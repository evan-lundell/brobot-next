using Brobot.Repositories;
using Brobot.Services;
using Discord.WebSocket;

namespace Brobot.Workers;

public class WordCloudWorker : CronWorkerBase
{
    private readonly IServiceProvider _services;
    private readonly ILogger<WordCloudWorker> _logger;

    public WordCloudWorker(
        ICronWorkerConfig<WordCloudWorker> config,
        IServiceProvider services,
        ILogger<WordCloudWorker> logger)
        : base(config.CronExpression)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var wordCloudService = scope.ServiceProvider.GetRequiredService<WordCloudService>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var eligibleChannels = await uow.Channels.Find((c) => c.Archived == false && c.MonthlyWordCloud);
            foreach (var channel in eligibleChannels)
            {
                await wordCloudService.GenerateWordCloud(channel);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Word Cloud failed");
        }
    }
}