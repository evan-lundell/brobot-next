using Brobot.Models;
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
        using var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var statsService = scope.ServiceProvider.GetRequiredService<IStatsService>();
        var channels = await uow.Channels.Find(c => c.MonthlyWordCloud);
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = now.AddMonths(-1);
        var endDate = now.AddDays(-1);
        foreach (var channel in channels)
        {
            StatPeriodModel statPeriod = new()
            {
                Channel = channel,
                ChannelId = channel.Id,
                StartDate = startDate,
                EndDate = endDate
            };
            await uow.StatPeriods.Add(statPeriod);
            await uow.CompleteAsync();
            var stats = await statsService.GetStats(channel,  startDate, endDate, statPeriod.Id); 
            await statsService.SendStats(channel.Id, stats);
        }
    }
}