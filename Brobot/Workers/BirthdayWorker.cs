using Brobot.Repositories;
using Discord.WebSocket;

namespace Brobot.Workers;

public class BirthdayWorker : CronWorkerBase
{
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;

    public BirthdayWorker(
        ICronWorkerConfig<BirthdayWorker> config,
        IServiceProvider services,
        DiscordSocketClient client) :
        base(config.CronExpression)
    {
        _services = services;
        _client = client;
    }

    public async override Task DoWork(CancellationToken cancellationToken)
    {
        using (var scope = _services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var now = DateTime.UtcNow;
            var users = await uow.Users.Find((u) =>
                u.Birthdate.HasValue
                && u.Birthdate.Value.Month == now.Month
                && u.Birthdate.Value.Day == now.Day
                && u.PrimaryChannelId.HasValue
            );

            var tasks = new List<Task>();
            foreach (var user in users)
            {
                var channel = (await _client.GetChannelAsync(user.PrimaryChannelId!.Value)) as SocketTextChannel;
                if (channel == null)
                {
                    continue;
                }
                tasks.Add(channel.SendMessageAsync($"Happy birthday {user.Username}! :birthday:"));
            }

            await Task.WhenAll(tasks);
        }
    }
}