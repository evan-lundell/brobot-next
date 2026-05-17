using Brobot.Models;
using Brobot.Services;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class MessageCountsController(IMessageCountService messageCountService) : ControllerBase
{
    [HttpGet("daily")]
    public async Task<ActionResult<IEnumerable<DailyMessageCountResponse>>> GetDailyMessageCounts(
        [FromQuery] int numOfDays = 10,
        [FromQuery] ulong? channelId = null,
        CancellationToken cancellationToken = default)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<DiscordUserModel>();
        if (string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            return Ok(Array.Empty<DailyMessageCountResponse>());
        }

        var counts = channelId == null
            ? await messageCountService.GetUsersTotalDailyMessageCounts(discordUser, numOfDays, cancellationToken)
            : await messageCountService.GetUsersDailyMessageCountForChannel(discordUser.Id, channelId.Value, numOfDays, cancellationToken);
        
        return Ok(counts);
    }

    [HttpGet("top-days")]
    public async Task<ActionResult<IEnumerable<DailyMessageCountResponse>>> GetUsersTopDays(
        [FromQuery] int numOfDays = 10,
        [FromQuery] ulong? channelId = null,
        CancellationToken cancellationToken = default)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<DiscordUserModel>();
        if (string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            return Ok(Array.Empty<DailyMessageCountResponse>());
        }

        var counts = channelId == null
            ? await messageCountService.GetUsersTopDays(discordUser, numOfDays, cancellationToken)
            : await messageCountService.GetUsersTopDaysByChannel(discordUser, channelId.Value, numOfDays, cancellationToken);
        return Ok(counts);
    }

    [HttpGet("top-today")]
    public async Task<ActionResult<IEnumerable<DailyMessageCountResponse>>> GetTopToday([FromQuery] ulong? channelId, CancellationToken cancellationToken = default)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<DiscordUserModel>();
        if (string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            return Ok(Array.Empty<DailyMessageCountResponse>());
        }

        var counts = channelId == null
            ? await messageCountService.GetTopToday(discordUser, cancellationToken)
            : await messageCountService.GetTopTodayByChannel(discordUser, channelId.Value, cancellationToken);
        return Ok(counts);
    }

    [HttpGet("total-daily")]
    public async Task<ActionResult<IEnumerable<DailyMessageCountResponse>>> GetTotalDailyMessageCounts(
        [FromQuery] int numOfDays = 10,
        [FromQuery] ulong? channelId = null,
        CancellationToken cancellationToken = default)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<DiscordUserModel>();
        var counts = channelId == null
            ? await messageCountService.GetTotalDailyMessageCounts(numOfDays, discordUser.Timezone, cancellationToken)
            : await messageCountService.GetTotalDailyMessageCountsByChannel(numOfDays, channelId.Value,
                discordUser.Timezone, cancellationToken);

        return Ok(counts);
    }

    [HttpGet("total-top-days")]
    public async Task<ActionResult<IEnumerable<DailyMessageCountResponse>>> GetTotalTopDays([FromQuery] int numOfDays = 10,
        [FromQuery] ulong? channelId = null,
        CancellationToken cancellationToken = default)
    {
        var counts = channelId == null
            ? await messageCountService.GetTotalTopDays(numOfDays, cancellationToken)
            : await messageCountService.GetTotalTopDaysByChannel(channelId.Value, numOfDays, cancellationToken);

        return Ok(counts);
    }
}