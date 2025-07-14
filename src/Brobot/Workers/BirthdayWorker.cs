using Brobot.Repositories;
using Discord;
using Discord.WebSocket;

// ReSharper disable ClassNeverInstantiated.Global

namespace Brobot.Workers;

public class BirthdayWorker(
    ICronWorkerConfig<BirthdayWorker> config,
    IServiceProvider services,
    IDiscordClient client)
    : CronWorkerBase(config.CronExpression)
{
    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var users = await uow.Users.Find(u =>
            u.Birthdate.HasValue
            && u.Birthdate.Value.Month == now.Month
            && u.Birthdate.Value.Day == now.Day
        );
        
        var tasks = new List<Task>();
        foreach (var user in users)
        {
            if (await client.GetChannelAsync(user.PrimaryChannelId!.Value) is not ISocketMessageChannel channel)
            {
                continue;
            }

            var socketUser = await client.GetUserAsync(user.Id);
            tasks.Add(channel.SendMessageAsync($"Happy birthday {socketUser?.Mention ?? user.Username}! :birthday:"));
        }
        
        await Task.WhenAll(tasks);
    }
}