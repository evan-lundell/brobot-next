namespace Brobot.TaskQueue;

public class QueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueuedHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await taskQueue.DequeueAsync(stoppingToken);
            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while executing {nameof(workItem)}.");
            }
        }
    }
}