using Brobot.Repositories;
using Brobot.Services;

namespace Brobot.Workers;

public class MonthlyStatsWorker(
    ICronWorkerConfig<MonthlyStatsWorker> config,
    IServiceScopeFactory serviceScopeFactory) : CronWorkerBase(config.CronExpression)
{
    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        using var scope =  serviceScopeFactory.CreateScope();
        using var iow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var statsService = scope.ServiceProvider.GetRequiredService<IStatsService>();
        var channels = await iow.Channels.Find(c => c.MonthlyWordCloud);
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = now.AddMonths(-1);
        var endDate = now.AddDays(-1);
        foreach (var channel in channels)
        {
           var stats = await statsService.GetStats(channel,  startDate, endDate);
           await statsService.SendStats(channel.Id, stats);
        }
    }
}