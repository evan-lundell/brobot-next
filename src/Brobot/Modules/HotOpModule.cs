using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Discord.Interactions;

namespace Brobot.Modules;

[Group("hotop", "Commands used to view and manage hot ops")]
public class HotOpModule (
    IUnitOfWork uow,
    IHotOpService hotOpService,
    ILogger<HotOpModule> logger) : InteractionModuleBase
{
    [SlashCommand("scoreboard", "Gets the scores for the active hot ops")]
    public async Task Scoreboard()
    {
        try
        {
            var activeHotOps = (await uow.HotOps.GetActiveHotOpsWithSessions(Context.Channel.Id)).ToArray();
            if (activeHotOps.Length == 0)
            {
                await RespondAsync("No active hot ops");
                return;
            }

            await RespondAsync(embeds: activeHotOps.Select(hotOpService.CreateScoreboardEmbed).ToArray());

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HotOp List command failed");
            await RespondAsync(text: "Failed to get hot op scores", ephemeral: true);
        }
    }

    [SlashCommand("create", "Creates a new hot op")]
    public async Task Create(
        [Summary("The starting time of the Hot Op")] DateTime startTime,
        [Summary("The ending time of the Hot Op")] DateTime endTime)
    {
        try
        {
            if (startTime >= endTime)
            {
                await RespondAsync(text: "Start time must be before end time", ephemeral: true);
                return;
            }
            
            var user = await uow.Users.GetByIdNoTracking(Context.User.Id);
            if (user == null)
            {
                logger.LogWarning("User {userId} not found", Context.User.Id);
                await RespondAsync(text: "Failed to create hot op", ephemeral: true);
                return;
            }
            
            var channel = await uow.Channels.GetById(Context.Channel.Id);
            if (channel == null)
            {
                logger.LogWarning("Channel {channelId} not found", Context.Channel.Id);
                await RespondAsync(text: "Failed to create hot op", ephemeral: true);
                return;
            }

            if (string.IsNullOrWhiteSpace(user.Timezone))
            {
                logger.LogWarning("User {userId} has no time zone", Context.User.Id);
                await RespondAsync("You must have a timezone set to create a Hot Op", ephemeral: true);
                return;
            }
            
            var startUtc = ((DateTimeOffset)startTime).AdjustToUtc(user.Timezone);
            var endUtc = ((DateTimeOffset)endTime).AdjustToUtc(user.Timezone);

            HotOpModel hotOp = new()
            {
                ChannelId = Context.Channel.Id,
                Channel = channel,
                DiscordUser = user,
                UserId = Context.User.Id,
                StartDate = startUtc,
                EndDate = endUtc
            };
            
            await uow.HotOps.Add(hotOp);
            await uow.CompleteAsync();

            await RespondAsync(text: $"Operation Hot {user.Username} has been created!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HotOp Create command failed");
            await RespondAsync(text: "Failed to create hot op", ephemeral: true);
        }
    }
}