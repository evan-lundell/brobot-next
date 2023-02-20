using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Brobot.Services;

public class DiscordEventHandler : IDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _services;
    private readonly InteractionService _commands;

    public DiscordEventHandler(
        DiscordSocketClient client,
        IServiceProvider services
    )
    {
        _client = client;
        _services = services;
        _commands = new InteractionService(client);
        _client.Log += Log;
        _client.Ready += Ready;
        _client.InteractionCreated += InteractionCreated;
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        var ctx = new SocketInteractionContext(_client, interaction);
        await _commands.ExecuteCommandAsync(ctx, _services);
    }

    private Task Ready()
    {
        var tasks = new List<Task>();
        using (var scope = _services.CreateScope())
        {
            tasks.Add(_commands.AddModulesAsync(Assembly.GetEntryAssembly(), scope.ServiceProvider));
            tasks.Add(_commands.RegisterCommandsToGuildAsync(421404457599762433));
        }

        return Task.WhenAll(tasks);
    }

    private Task Log(LogMessage logMessage)
    {
        Console.WriteLine(logMessage.Message);
        if (logMessage.Exception != null)
        {
            Console.WriteLine(logMessage.Exception.ToString());
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _client.Log -= Log;
    }
}