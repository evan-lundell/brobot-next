using Brobot.Repositories;
using Discord.WebSocket;

namespace Brobot.Workers;

public class ReminderWorker : CronWorkerBase
{
    private readonly IServiceProvider _services;
    private DiscordSocketClient _client;

    public ReminderWorker(ICronWorkerConfig<ReminderWorker> config, IServiceProvider services, DiscordSocketClient client)
        : base(config.CronExpression)
    {
        _services = services;
        _client = client;
    }

    public async override Task DoWork(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Checking reminders at {DateTime.Now.ToString()}");
        using (var scope = _services.CreateScope())
        {
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
}