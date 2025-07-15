using Discord;
using Discord.WebSocket;

namespace Brobot.HostedServices;

public class DiscordBotHostedService(
    IDiscordClient client,
    IConfiguration config,
    IServiceProvider serviceProvider,
    ILogger<DiscordBotHostedService> logger) : IHostedService
{
    private DiscordEventHandler? _eventHandler;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (config["NoBot"] == bool.TrueString)
        {
            logger.LogDebug("NoBot is enabled");
            return;
        }
        var token = config["BrobotToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogCritical("No token found, exiting");
            throw new InvalidOperationException("No Discord bot token configured.");
        }

        if (client is not DiscordSocketClient socketClient)
        {
            logger.LogCritical("No Discord socket client configured, exiting");
            throw new InvalidOperationException("Discord bot client is not discord socket client.");
        }

        _eventHandler = new DiscordEventHandler(socketClient, serviceProvider);
        _eventHandler.RegisterEvents();

        logger.LogInformation("Starting Discord bot");
        
        await socketClient.LoginAsync(TokenType.Bot, token);

        // This will throw if connection fails
        await socketClient.StartAsync();

        // Optionally, wait for Ready event
        var tcs = new TaskCompletionSource<bool>();
        Task ReadyHandler()
        {
            tcs.SetResult(true);
            return Task.CompletedTask;
        }

        socketClient.Ready += ReadyHandler;

        // Wait up to 10 seconds
        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10), cancellationToken));
        socketClient.Ready -= ReadyHandler;

        if (completedTask != tcs.Task)
        {
            logger.LogCritical("Failed to start Discord bot. Exiting");
            throw new TimeoutException("Discord client did not become ready in time.");
        }
        
        logger.LogInformation("Discord bot started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _eventHandler?.Dispose();
        await client.StopAsync();
    }
}