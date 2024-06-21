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

#pragma warning disable CA1816
    public virtual void Dispose()
#pragma warning restore CA1816
    {
        _timer?.Dispose();
    }

    private async Task ScheduleJob(CancellationToken cancellationToken)
    {
        try
        {
            var next = _expression.GetNextOccurrence(DateTime.UtcNow);
            if (next.HasValue)
            {
                var delay = next.Value - DateTime.UtcNow;
                if (delay.TotalMilliseconds <= 0)
                {
                    await ScheduleJob(cancellationToken);
                }

                // Timer only supports up to int.MaxValue. If delay exceeds that, delay the max value and do nothing
                if (delay.TotalMilliseconds > int.MaxValue)
                {
                    _timer = new System.Timers.Timer(int.MaxValue);
                    _timer.Elapsed += (_, _) =>
                    {
                        _timer.Dispose();
                        _timer = null;
                    };
                }
                else
                {
                    // Sometimes the minutely cron job fires a second early, so adding a second to the timer to offset
                    _timer = new System.Timers.Timer(delay.TotalMilliseconds + 1000);
                    _timer.Elapsed += async (_, _) =>
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
                }

                _timer.Start();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        await Task.CompletedTask;
    }

    protected abstract Task DoWork(CancellationToken cancellationToken);
}
