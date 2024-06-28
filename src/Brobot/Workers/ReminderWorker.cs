using Brobot.Repositories;
using Discord.WebSocket;

// ReSharper disable ClassNeverInstantiated.Global

namespace Brobot.Workers;

public class ReminderWorker : CronWorkerBase
{
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;

    public ReminderWorker(ICronWorkerConfig<ReminderWorker> config, IServiceProvider services, DiscordSocketClient client)
        : base(config.CronExpression)
    {
        _services = services;
        _client = client;
    }

    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var messages = await uow.ScheduledMessages.GetActiveMessages();
        foreach (var message in messages)
        {
            var channel = await _client.GetChannelAsync(message.ChannelId);
            if (!(channel is SocketTextChannel textChannel))
            {
                continue;
            }
            await textChannel.SendMessageAsync(message.MessageText);
            message.SentDate = DateTime.UtcNow;
        }

        await uow.CompleteAsync();
    }
}