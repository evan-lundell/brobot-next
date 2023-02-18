using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Brobot.Services;

public class DiscordEventHandler : IDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;

    public DiscordEventHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
    {
        _client = client;
        _commands = commands;
        _services = services;

        _client.Log += Log;
        _client.InteractionCreated += InteractionCreated;
    }

    public async Task StartAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private Task Log(LogMessage logMessage)
    {
        Console.WriteLine(logMessage.Message);
        return Task.CompletedTask;
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        var ctx = new SocketInteractionContext(_client, interaction);
        await _commands.ExecuteCommandAsync(ctx, _services);
    }

    public void Dispose()
    {
        _client.Log -= Log;
        _client.InteractionCreated -= InteractionCreated;
    }
}