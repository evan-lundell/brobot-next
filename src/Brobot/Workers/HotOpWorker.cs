using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Discord.WebSocket;

// ReSharper disable ClassNeverInstantiated.Global

namespace Brobot.Workers;

public class HotOpWorker(
    ICronWorkerConfig<HotOpWorker> config,
    IServiceScopeFactory serviceScopeFactory,
    IDiscordClient client,
    ILogger<HotOpWorker> logger,
    TimeProvider timeProvider)
    : CronWorkerBase(config.CronExpression, logger, timeProvider)
{
    protected override async Task DoWork(DateTimeOffset target, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var hotOpService = scope.ServiceProvider.GetRequiredService<IHotOpService>();
        var utcTarget = target.ToUniversalTime();
        var minuteStart = new DateTimeOffset(utcTarget.Year, utcTarget.Month, utcTarget.Day, utcTarget.Hour, utcTarget.Minute, 0, TimeSpan.Zero);

        var startingHotOps = await uow.HotOps.Find(
            ho => minuteStart <= ho.StartDate && minuteStart.AddMinutes(1) > ho.StartDate,
            cancellationToken);
        foreach (var startingHotOp in startingHotOps)
        {
            await HandleStartingHotOp(startingHotOp, hotOpService, cancellationToken);
        }

        var endingHotOps = await uow.HotOps.Find(
            ho => minuteStart <= ho.EndDate && minuteStart.AddMinutes(1) > ho.EndDate,
            cancellationToken);
        foreach (var endingHotOp in endingHotOps)
        {
            await HandleEndingHotOp(endingHotOp, uow, hotOpService, cancellationToken);
        }
    }

    private async Task HandleStartingHotOp(HotOpModel hotOp, IHotOpService hotOpService, CancellationToken cancellationToken)
    {
        try
        {
            // If the owner of the hot op is currently in a voice channel, start sessions with everyone else
            cancellationToken.ThrowIfCancellationRequested();
            var guild = await client.GetGuildAsync(hotOp.Channel.GuildId);
            if (guild is not null)
            {
                var user = await guild.GetUserAsync(hotOp.UserId);
                if (user is not null)
                {
                    if (user.VoiceChannel != null)
                    {
                        var connectedUsers = await user.VoiceChannel.GetConnectedUsersAsync();
                        await hotOpService.CreateSessions(hotOp, [.. connectedUsers.Select(cu => cu.Id)], hotOp.StartDate, cancellationToken);
                    }
                }
                else
                {
                    logger.LogWarning("User {UserId} not found, skipping Hot Op {HotOpId}", hotOp.UserId, hotOp.Id);
                }
            }
            else
            {
                logger.LogWarning("Guild {GuildId} not found, skipping Hot Op {HotOpId}", hotOp.Channel.GuildId, hotOp.Id);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var channel = await client.GetChannelAsync(hotOp.ChannelId);
            if (channel is ISocketMessageChannel textChannel)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await textChannel.SendMessageAsync($"Operation Hot {hotOp.DiscordUser.Username} has begun!");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HandleStartingHotOp failed for Hot Op {HotOpId}", hotOp.Id);
        }
    }

    private async Task HandleEndingHotOp(HotOpModel hotOp, IUnitOfWork uow, IHotOpService hotOpService, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var openSession in hotOp.HotOpSessions.Where(hos => hos.EndDateTime == null))
            {
                openSession.EndDateTime = hotOp.EndDate;
            }
            await uow.CompleteAsync(cancellationToken);

            var channel = await client.GetChannelAsync(hotOp.ChannelId);
            if (channel is ISocketMessageChannel textChannel)
            {
                var scoreboardEmbed = hotOpService.CreateScoreboardEmbed(hotOp);
                cancellationToken.ThrowIfCancellationRequested();
                await textChannel.SendMessageAsync(text: $"Operation Hot {hotOp.DiscordUser.Username} has ended!", embed: scoreboardEmbed);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HandleEndingHotOp failed for Hot Op {HotOpId}", hotOp.Id);
        }
    }
}