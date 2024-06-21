using Brobot.Models;
using Brobot.Services;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class MessageCountsController : ControllerBase
{
    private readonly MessageCountService _messageCountService;
    
    public MessageCountsController(MessageCountService messageCountService)
    {
        _messageCountService = messageCountService;
    }

    [HttpGet("daily")]
    public async Task<ActionResult<IEnumerable<DailyMessageCountResponse>>> GetDailyMessageCounts([FromQuery] int numOfDays = 10, [FromQuery] ulong? channelId = null)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            return Ok(Array.Empty<DailyMessageCountResponse>());
        }

        var counts = channelId == null
            ? await _messageCountService.GetUsersTotalDailyMessageCounts(discordUser, numOfDays)
            : await _messageCountService.GetUsersDailyMessageCountForChannel(discordUser.Id, channelId.Value, numOfDays);
        
        return Ok(counts);
    }

    [HttpGet("top-days")]
    public async Task<ActionResult<IEnumerable<DailyMessageCountResponse>>> GetUsersTopDays([FromQuery] int numOfDays = 10, [FromQuery] ulong? channelId = null)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            return Ok(Array.Empty<DailyMessageCountResponse>());
        }

        var counts = channelId == null
            ? await _messageCountService.GetUsersTopDays(discordUser, numOfDays)
            : await _messageCountService.GetUsersTopDaysByChannel(discordUser, channelId.Value, numOfDays);
        return Ok(counts);
    }

    [HttpGet("top-today")]
    public async Task<ActionResult<IEnumerable<DailyMessageCountResponse>>> GetTopToday([FromQuery] ulong? channelId)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            return Ok(Array.Empty<DailyMessageCountResponse>());
        }

        var counts = channelId == null
            ? await _messageCountService.GetTopToday(discordUser)
            : await _messageCountService.GetTopTodayByChannel(discordUser, channelId.Value);
        return Ok(counts);
    }

    [HttpGet("total-daily")]
    public async Task<ActionResult<IEnumerable<DailyMessageCountResponse>>> GetTotalDailyMessageCounts(
        [FromQuery] int numOfDays = 10, [FromQuery] ulong? channelId = null)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var counts = channelId == null
            ? await _messageCountService.GetTotalDailyMessageCounts(numOfDays, discordUser.Timezone)
            : await _messageCountService.GetTotalDailyMessageCountsByChannel(numOfDays, channelId.Value,
                discordUser.Timezone);

        return Ok(counts);
    }

    [HttpGet("total-top-days")]
    public async Task<ActionResult<IEnumerable<DailyMessageCountResponse>>> GetTotalTopDays([FromQuery] int numOfDays = 10, [FromQuery] ulong? channelId = null)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel)
        {
            return Unauthorized();
        }

        var counts = channelId == null
            ? await _messageCountService.GetTotalTopDays(numOfDays)
            : await _messageCountService.GetTotalTopDaysByChannel(channelId.Value, numOfDays);

        return Ok(counts);
    }
}