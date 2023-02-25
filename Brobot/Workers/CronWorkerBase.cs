using Cronos;

namespace Brobot.Workers;

public abstract class CronWorkerBase : IHostedService, IDisposable
{
    private System.Timers.Timer? _timer;
    private readonly CronExpression _expression;

    protected CronWorkerBase(string? cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new ArgumentNullException(nameof(cronExpression), "cron expression cannot be null");
        }
        _expression = CronExpression.Parse(cronExpression);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return ScheduleJob(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Stop();
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        _timer?.Dispose();
    }

    protected async Task ScheduleJob(CancellationToken cancellationToken)
    {
        var next = _expression.GetNextOccurrence(DateTime.UtcNow);
        if (next.HasValue)
        {
            var delay = next.Value - DateTime.UtcNow;
            if (delay.TotalMilliseconds <= 0)
            {
                await ScheduleJob(cancellationToken);
            }
            // Sometimes the minutely cron job fires a second early, so adding a second to the timer to offset
            _timer = new System.Timers.Timer(delay.TotalMilliseconds + 1000);
            _timer.Elapsed += async (sender, args) =>
            {
                _timer.Dispose();
                _timer = null;

                if (!cancellationToken.IsCancellationRequested)
                {
                    await DoWork(cancellationToken);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    await ScheduleJob(cancellationToken);
                }
            };

            _timer.Start();
        }
        await Task.CompletedTask;
    }

    public abstract Task DoWork(CancellationToken cancellationToken);

}
