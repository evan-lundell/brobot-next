using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;

namespace Brobot.Workers;

public class WordCountWorker : CronWorkerBase
{
    private readonly IServiceProvider _services;
    private readonly ILogger<WordCountWorker> _logger;
    private readonly WordCountService _wordCountService;

    public WordCountWorker(
        ICronWorkerConfig<WordCountWorker> config,
        IServiceProvider services,
        ILogger<WordCountWorker> logger,
        WordCountService wordCountService)
        : base(config.CronExpression)
    {
        _services = services;
        _logger = logger;
        _wordCountService = wordCountService;
    }

    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var channels = await uow.Channels.GetAll();
            var wordCounts = new List<WordCountModel>();
            foreach (var channel in channels)
            {
                wordCounts.AddRange(await _wordCountService.GetWordCount(channel));
            }
            
            await uow.WordCounts.AddRange(wordCounts);
            await uow.CompleteAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Word Count failed");
        }
    }
}