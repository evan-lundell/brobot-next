using Brobot.Repositories;
using Discord;
using Discord.WebSocket;

// ReSharper disable ClassNeverInstantiated.Global

namespace Brobot.Workers;

public class BirthdayWorker(
    ICronWorkerConfig<BirthdayWorker> config,
    IServiceScopeFactory serviceScopeFactory,
    IDiscordClient client,
    ILogger<BirthdayWorker> logger,
    TimeProvider timeProvider)
    : CronWorkerBase(config.CronExpression, logger, timeProvider)
{
    protected override async Task DoWork(DateTimeOffset target, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var now = DateOnly.FromDateTime(target.UtcDateTime);
        var users = await uow.Users.Find(u =>
            u.Birthdate.HasValue
            && u.Birthdate.Value.Month == now.Month
            && u.Birthdate.Value.Day == now.Day,
            cancellationToken
        );
        
        var tasks = new List<Task>();
        foreach (var user in users)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!user.PrimaryChannelId.HasValue || await client.GetChannelAsync(user.PrimaryChannelId.Value) is not ISocketMessageChannel channel)
            {
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var socketUser = await client.GetUserAsync(user.Id);
            tasks.Add(channel.SendMessageAsync($"Happy birthday {socketUser?.Mention ?? user.Username}! :birthday:"));
        }
        
        cancellationToken.ThrowIfCancellationRequested();
        await Task.WhenAll(tasks);
    }
}