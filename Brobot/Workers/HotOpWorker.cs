using Brobot.Dtos;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Discord.WebSocket;

namespace Brobot.Workers;

public class HotOpWorker : CronWorkerBase
{
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;
    private readonly HotOpService _hotOpService;

    public HotOpWorker(
        ICronWorkerConfig<HotOpWorker> config,
        IServiceProvider services,
        DiscordSocketClient client,
        HotOpService hotOpService) : base(config.CronExpression)
    {
        _services = services;
        _client = client;
        _hotOpService = hotOpService;
    }

    public async override Task DoWork(CancellationToken cancellationToken)
    {
        using (var scope = _services.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var now = DateTime.UtcNow;
            var minuteStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);

            var newHotOps = await uow.HotOps.Find((ho) => minuteStart <= ho.StartDate && minuteStart.AddMinutes(1) > ho.StartDate);
            foreach (var newHotOp in newHotOps)
            {
                await HandleNewHotOp(newHotOp);
            }

            var endingHotOps = await uow.HotOps.Find((ho) => minuteStart <= ho.EndDate && minuteStart.AddMinutes(1) > ho.EndDate);
            foreach (var endingHotOp in endingHotOps)
            {
                await HandleEndingHotOp(endingHotOp, uow);
            }
        }
    }

    private async Task HandleNewHotOp(HotOpModel hotOp)
    {
        var channel = await _client.GetChannelAsync(hotOp.ChannelId);
        if (channel is SocketTextChannel textChannel)
        {
            await textChannel.SendMessageAsync($"Operation Hot {hotOp.User.Username} has begun!");
        }
    }

    private async Task HandleEndingHotOp(HotOpModel hotOp, IUnitOfWork uow)
    {
        var now = DateTime.UtcNow;
        foreach (var openSession in hotOp.HotOpSessions.Where((hos) => hos.EndDateTime == null))
        {
            openSession.EndDateTime = now;
        }
        await uow.CompleteAsync();

        var channel = await _client.GetChannelAsync(hotOp.ChannelId);
        if (channel is SocketTextChannel textChannel)
        {
            var scoreboardEmbed = _hotOpService.CreateScoreboardEmbed(hotOp);
            await textChannel.SendMessageAsync(text: $"Operation Hot {hotOp.User.Username} has ended!", embed: scoreboardEmbed);
        }
    }
}