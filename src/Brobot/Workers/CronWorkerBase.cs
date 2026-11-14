using Cronos;

namespace Brobot.Workers;

public abstract class CronWorkerBase : BackgroundService
{
    private readonly CronExpression _expression;
    private readonly ILogger _logger;
    private readonly TimeProvider _timeProvider;

    protected virtual TimeSpan MaxDelay => TimeSpan.FromDays(30);

    protected CronWorkerBase(string? cronExpression, ILogger logger, TimeProvider timeProvider)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new ArgumentNullException(nameof(cronExpression), "cron expression cannot be null");
        }
        _expression = CronExpression.Parse(cronExpression);
        _logger = logger;
        _timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DateTimeOffset? lastTarget = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = _timeProvider.GetUtcNow();
            var from = lastTarget is { } last && last > now ? last : now;
            var next = _expression.GetNextOccurrence(from, TimeZoneInfo.Utc);
            if (next is null)
            {
                return;
            }

            await WaitUntil(next.Value, stoppingToken);
            lastTarget = next.Value;

            try
            {
                await DoWork(next.Value, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute job");
            }
        }
    }

    private async Task WaitUntil(DateTimeOffset target, CancellationToken ct)
    {
        while (true)
        {
            var remaining = target - _timeProvider.GetUtcNow();
            if (remaining <= TimeSpan.Zero)
            {
                return;
            }
            await Task.Delay(remaining < MaxDelay ? remaining : MaxDelay, _timeProvider, ct);
        }
    }

    protected abstract Task DoWork(DateTimeOffset target, CancellationToken cancellationToken);
}
