using Brobot.Repositories;
using Discord;
using Discord.WebSocket;

// ReSharper disable ClassNeverInstantiated.Global

namespace Brobot.Workers;

public class ReminderWorker(
    ICronWorkerConfig<ReminderWorker> config,
    IServiceProvider services,
    IDiscordClient client)
    : CronWorkerBase(config.CronExpression)
{
    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var messages = await uow.ScheduledMessages.GetActiveMessages();
        foreach (var message in messages)
        {
            var channel = await client.GetChannelAsync(message.ChannelId);
            if (channel is not ISocketMessageChannel textChannel)
            {
                continue;
            }
            await textChannel.SendMessageAsync(message.MessageText);
            message.SentDate = DateTime.UtcNow;
        }

        await uow.CompleteAsync();
    }
}