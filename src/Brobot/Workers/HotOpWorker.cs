using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Discord.WebSocket;

// ReSharper disable ClassNeverInstantiated.Global

namespace Brobot.Workers;

public class HotOpWorker(
    ICronWorkerConfig<HotOpWorker> config,
    IServiceProvider services,
    IDiscordClient client)
    : CronWorkerBase(config.CronExpression)
{
    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var hotOpService = scope.ServiceProvider.GetRequiredService<IHotOpService>();
        var now = DateTime.UtcNow;
        var minuteStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);

        var newHotOps = await uow.HotOps.Find(ho => minuteStart <= ho.StartDate && minuteStart.AddMinutes(1) > ho.StartDate);
        foreach (var newHotOp in newHotOps)
        {
            await HandleNewHotOp(newHotOp);
        }

        var endingHotOps = await uow.HotOps.Find(ho => minuteStart <= ho.EndDate && minuteStart.AddMinutes(1) > ho.EndDate);
        foreach (var endingHotOp in endingHotOps)
        {
            await HandleEndingHotOp(endingHotOp, uow, hotOpService);
        }
    }

    private async Task HandleNewHotOp(HotOpModel hotOp)
    {
        var channel = await client.GetChannelAsync(hotOp.ChannelId);
        if (channel is ISocketMessageChannel textChannel)
        {
            await textChannel.SendMessageAsync($"Operation Hot {hotOp.User.Username} has begun!");
        }
    }

    private async Task HandleEndingHotOp(HotOpModel hotOp, IUnitOfWork uow, IHotOpService hotOpService)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var openSession in hotOp.HotOpSessions.Where(hos => hos.EndDateTime == null))
        {
            openSession.EndDateTime = now;
        }
        await uow.CompleteAsync();

        var channel = await client.GetChannelAsync(hotOp.ChannelId);
        if (channel is ISocketMessageChannel textChannel)
        {
            var scoreboardEmbed = hotOpService.CreateScoreboardEmbed(hotOp);
            await textChannel.SendMessageAsync(text: $"Operation Hot {hotOp.User.Username} has ended!", embed: scoreboardEmbed);
        }
    }
}