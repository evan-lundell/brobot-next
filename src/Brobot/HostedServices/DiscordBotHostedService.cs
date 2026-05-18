using Brobot.Configuration;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Brobot.HostedServices;

public class DiscordBotHostedService(
    IDiscordClient client,
    IOptions<DiscordOptions> options,
    IServiceProvider serviceProvider,
    ILogger<DiscordBotHostedService> logger) : IHostedService
{
    private DiscordEventHandler? _eventHandler;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (client is not DiscordSocketClient socketClient)
        {
            logger.LogCritical("No Discord socket client configured, exiting");
            throw new InvalidOperationException("Discord bot client is not discord socket client.");
        }

        _eventHandler = new DiscordEventHandler(socketClient, serviceProvider);
        _eventHandler.RegisterEvents();

        logger.LogInformation("Starting Discord bot");

        var readyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task OnReady()
        {
            readyTcs.TrySetResult(true);
            return Task.CompletedTask;
        }

        socketClient.Ready += OnReady;

        try
        {
            await socketClient.LoginAsync(TokenType.Bot, options.Value.BrobotToken);
            await socketClient.StartAsync();

            var completed = await Task.WhenAny(
                readyTcs.Task,
                Task.Delay(TimeSpan.FromSeconds(10), cancellationToken));

            if (completed != readyTcs.Task)
            {
                throw new TimeoutException("Discord client did not become ready in time.");
            }
        }
        finally
        {
            socketClient.Ready -= OnReady;
        }
        
        logger.LogInformation("Discord bot started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _eventHandler?.Dispose();
        await client.StopAsync();
    }
}