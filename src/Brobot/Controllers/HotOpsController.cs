using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Brobot.Mappers;
using TimeZoneConverter;

namespace Brobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HotOpsController(
    IUnitOfWork uow,
    IHotOpService hotOpService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HotOpResponse>>> GetHotOps([FromQuery] HotOpQueryType type = HotOpQueryType.All)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        IEnumerable<HotOpModel>? hotOpModels;
        switch (type)
        {
            case HotOpQueryType.Upcoming:
                var now = DateTimeOffset.UtcNow;
                 hotOpModels = await uow.HotOps.Find(ho => ho.UserId == discordUser.Id && ho.StartDate > now);
                break;
            case HotOpQueryType.Current:
                hotOpModels = await uow.HotOps.GetUsersHotOps(discordUser.Id, HotOpQueryType.Current);
                break;
            case HotOpQueryType.Completed:
                hotOpModels = await uow.HotOps.GetUsersHotOps(discordUser.Id, HotOpQueryType.Completed);
                break;
            default:
                hotOpModels = await uow.HotOps.Find(ho => ho.UserId == discordUser.Id);
                break;
        }

        var hotOps = hotOpModels.Select(h => h.ToHotOpResponse()).ToArray();
        if (!string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            foreach (var hotOp in hotOps)
            {
                hotOp.StartDate = hotOp.StartDate.AdjustToUsersTimezone(discordUser.Timezone);
                hotOp.EndDate = hotOp.EndDate.AdjustToUsersTimezone(discordUser.Timezone);
            }
        }
        
        return Ok(hotOps);
    }
    
    [HttpPost]
    public async Task<ActionResult<HotOpResponse>> CreateHotOp(HotOpRequest hotOpRequest)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        if (hotOpRequest.StartDate >= hotOpRequest.EndDate)
        {
            return BadRequest("Start date must be before end date");
        }
        
        var channel = await uow.Channels.GetById(hotOpRequest.ChannelId);
        if (channel == null)
        {
            return BadRequest("Invalid channel");
        }

        var offset = TimeSpan.Zero;
        var startDateUnspecified = new DateTime(hotOpRequest.StartDate.Ticks, DateTimeKind.Unspecified);
        var endDateUnspecified = new DateTime(hotOpRequest.EndDate.Ticks, DateTimeKind.Unspecified);
        if (!string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            var timezone = TZConvert.GetTimeZoneInfo(discordUser.Timezone);
            offset = timezone.GetUtcOffset(DateTime.Now);
        }
        var startDateAdjusted = new DateTimeOffset(startDateUnspecified, offset).ToUniversalTime();
        var endDateAdjusted = new DateTimeOffset(endDateUnspecified, offset).ToUniversalTime();
        
        var hotOpModel = new HotOpModel
        {
            UserId = discordUser.Id,
            User = discordUser,
            Channel = channel,
            ChannelId = channel.Id,
            StartDate = startDateAdjusted,
            EndDate = endDateAdjusted,
        };

        await uow.HotOps.Add(hotOpModel);
        await uow.CompleteAsync();
        var hotOpResponse = hotOpModel.ToHotOpResponse();
        if (!string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            hotOpResponse.StartDate = hotOpResponse.StartDate.AdjustToUsersTimezone(discordUser.Timezone);
            hotOpResponse.EndDate = hotOpResponse.EndDate.AdjustToUsersTimezone(discordUser.Timezone);
        }

        return Ok(hotOpResponse);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<HotOpResponse>> UpdateHotOp(int id, HotOpRequest hotOpRequest)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        var hotOpModel = await uow.HotOps.GetById(id);
        if (hotOpModel == null)
        {
            return NotFound("Hot Op not found");
        }

        if (hotOpModel.UserId != discordUser.Id)
        {
            return Unauthorized();
        }

        if (hotOpRequest.StartDate >= hotOpRequest.EndDate)
        {
            return BadRequest("Start date must be before end date");
        }
        
        var channel = await uow.Channels.GetById(hotOpRequest.ChannelId);
        if (channel == null)
        {
            return BadRequest("Invalid channel");
        }

        var adjustedTimes = (hotOpRequest.StartDate, hotOpRequest.EndDate);
        if (!string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            adjustedTimes.StartDate = adjustedTimes.StartDate.AdjustToUtc(discordUser.Timezone);
            adjustedTimes.EndDate = adjustedTimes.EndDate.AdjustToUtc(discordUser.Timezone);
        }
        hotOpModel.ChannelId = hotOpRequest.ChannelId;
        hotOpModel.Channel = channel;
        hotOpModel.StartDate = adjustedTimes.StartDate;
        hotOpModel.EndDate = adjustedTimes.EndDate;
        await uow.CompleteAsync();
        var hotOpResponse = hotOpModel.ToHotOpResponse();
        if (!string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            hotOpResponse.StartDate = hotOpResponse.StartDate.AdjustToUsersTimezone(discordUser.Timezone);
            hotOpResponse.EndDate = hotOpResponse.EndDate.AdjustToUsersTimezone(discordUser.Timezone);

        }
        return Ok(hotOpResponse);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHotOp(int id)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        var hotOpModel = await uow.HotOps.GetById(id);
        if (hotOpModel == null)
        {
            return NotFound("Hot Op not found");
        }

        if (hotOpModel.UserId != discordUser.Id)
        {
            return Unauthorized();
        }

        uow.HotOps.Remove(hotOpModel);
        await uow.CompleteAsync();
        return Ok();
    }

    [HttpGet("{id:int}/scoreboard")]
    public async Task<ActionResult<IEnumerable<ScoreboardItemResponse>>> GetHotOpScoreboard(int id)
    {
        var hotOp = await uow.HotOps.GetById(id);
        if (hotOp == null)
        {
            return NotFound();
        }

        if (hotOp.StartDate > DateTimeOffset.UtcNow)
        {
            return BadRequest("Hot Op hasn't started yet");
        }

        var scoreboard = hotOpService.GetScoreboard(hotOp);
        var scoreboardResponse = scoreboard.ToScoreboardResponse();
        return Ok(scoreboardResponse);
    }
}