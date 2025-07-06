using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;

namespace Brobot.Workers;

public class WordCountWorker(
    ICronWorkerConfig<WordCountWorker> config,
    IServiceProvider services,
    ILogger<WordCountWorker> logger,
    WordCountService wordCountService)
    : CronWorkerBase(config.CronExpression)
{
    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var channels = await uow.Channels.GetAll();
            var wordCounts = new List<WordCountModel>();
            foreach (var channel in channels)
            {
                wordCounts.AddRange(await wordCountService.GetWordCount(channel));
            }
            
            await uow.WordCounts.AddRange(wordCounts);
            await uow.CompleteAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Word Count failed");
        }
    }
}