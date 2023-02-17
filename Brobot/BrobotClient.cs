using Discord;
using Discord.WebSocket;

namespace Brobot;

public class BrobotClient : IDisposable
{
    public DiscordSocketClient Client { get; private set; }

    public BrobotClient()
    {
        Client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = Discord.GatewayIntents.AllUnprivileged | Discord.GatewayIntents.GuildPresences
        });

        Client.Log += Log;
        Client.PresenceUpdated += PresenceUpdated;
    }

    public async Task StartClientAsync(string token)
    {
        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();
    }

    private Task Log(LogMessage logMessage)
    {
        Console.WriteLine(logMessage.Message);
        return Task.CompletedTask;
    }

    private Task PresenceUpdated(SocketUser user, SocketPresence previousPresence, SocketPresence newPresence)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Client.Log -= Log;
        Client.PresenceUpdated -= PresenceUpdated;
    }
}