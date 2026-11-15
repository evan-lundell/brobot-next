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
    IHotOpService hotOpService,
    ILogger<HotOpsController> logger) : ControllerBase
{
    private bool IsAdmin => User.IsInRole(Constants.AdminRoleName);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HotOpResponse>>> GetHotOps(
        [FromQuery] HotOpQueryType type = HotOpQueryType.All,
        [FromQuery] bool allUsers = false,
        CancellationToken cancellationToken = default)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<DiscordUserModel>();

        if (allUsers && !IsAdmin)
        {
            return Forbid();
        }

        var now = DateTimeOffset.UtcNow;
        var hotOps = (type switch
        {
            HotOpQueryType.Upcoming => await uow.HotOps.Find(ho => (allUsers || ho.UserId == discordUser.Id) && ho.StartDate > now, cancellationToken),
            HotOpQueryType.Current => await uow.HotOps.GetUsersHotOps(discordUser.Id, HotOpQueryType.Current, cancellationToken),
            HotOpQueryType.Completed => await uow.HotOps.GetUsersHotOps(discordUser.Id, HotOpQueryType.Completed, cancellationToken),
            _ => await uow.HotOps.Find(ho => ho.UserId == discordUser.Id, cancellationToken)
        }).Select(hotOp => hotOp.ToHotOpResponse());
        
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
    public async Task<ActionResult<HotOpResponse>> CreateHotOp(HotOpRequest hotOpRequest, CancellationToken cancellationToken = default)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<DiscordUserModel>();

        if (hotOpRequest.StartDate >= hotOpRequest.EndDate)
        {
            logger.LogWarning("Start date must be before end date");
            return BadRequest("Start date must be before end date");
        }

        if (hotOpRequest.StartDate.Ticks % TimeSpan.TicksPerMinute != 0)
        {
            logger.LogWarning("StartDate seconds must be set to 0");
            return BadRequest("Invalid time, Hot Ops must begin on the minute");
        }

        if (hotOpRequest.EndDate.Ticks % TimeSpan.TicksPerMinute != 0)
        {
            logger.LogWarning("EndDate seconds must be set to 0");
            return BadRequest("Invalid time, Hot Ops must end on the minute");
        }

        if (hotOpRequest.UserId is not null && hotOpRequest.UserId != discordUser.Id && !IsAdmin)
        {
            logger.LogWarning("User {UserId} attempting to create a hot op for {HotOpUserId} and is not an admin", discordUser.Id, hotOpRequest.UserId);
            return Forbid();
        }

        var owner = await GetOwner(hotOpRequest, discordUser, cancellationToken);
        if (owner is null)
        {
            logger.LogWarning("Unable to find user {UserId}", discordUser.Id);
            return BadRequest("Invalid user");
        }
        
        var channel = await uow.Channels.GetById(hotOpRequest.ChannelId, cancellationToken);
        if (channel == null)
        {
            logger.LogWarning("Channel not found");
            return BadRequest("Invalid channel");
        }

        var startOffset = TimeSpan.Zero;
        var endOffset = TimeSpan.Zero;
        var startDateUnspecified = new DateTime(hotOpRequest.StartDate.Ticks, DateTimeKind.Unspecified);
        var endDateUnspecified = new DateTime(hotOpRequest.EndDate.Ticks, DateTimeKind.Unspecified);
        if (!string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            var timezone = TZConvert.GetTimeZoneInfo(discordUser.Timezone);
            startOffset = timezone.GetUtcOffset(startDateUnspecified);
            endOffset = timezone.GetUtcOffset(endDateUnspecified);
        }
        var startDateAdjusted = new DateTimeOffset(startDateUnspecified, startOffset).ToUniversalTime();
        var endDateAdjusted = new DateTimeOffset(endDateUnspecified, endOffset).ToUniversalTime();
        
        var hotOpModel = new HotOpModel
        {
            UserId = owner.Id,
            DiscordUser = owner,
            Channel = channel,
            ChannelId = channel.Id,
            StartDate = startDateAdjusted,
            EndDate = endDateAdjusted,
        };

        await uow.HotOps.Add(hotOpModel, cancellationToken);
        await uow.CompleteAsync(cancellationToken);
        var hotOpResponse = AdjustDateTimes(hotOpModel.ToHotOpResponse(), discordUser);
        return Ok(hotOpResponse);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<HotOpResponse>> UpdateHotOp(int id, HotOpRequest hotOpRequest, CancellationToken cancellationToken = default)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<DiscordUserModel>();
        var hotOpModel = await uow.HotOps.GetById(id, cancellationToken);
        if (hotOpModel == null)
        {
            logger.LogWarning("Hot Op {HotOpId} not found", id);
            return NotFound("Hot Op not found");
        }

        if (hotOpRequest.StartDate >= hotOpRequest.EndDate)
        {
            logger.LogWarning("Start date must be before end date");
            return BadRequest("Start date must be before end date");
        }

        if (hotOpRequest.StartDate.Ticks % TimeSpan.TicksPerMinute != 0)
        {
            logger.LogWarning("StartDate seconds must be set to 0");
            return BadRequest("Invalid time, Hot Ops must begin on the minute");
        }

        if (hotOpRequest.EndDate.Ticks % TimeSpan.TicksPerMinute != 0)
        {
            logger.LogWarning("EndDate seconds must be set to 0");
            return BadRequest("Invalid time, Hot Ops must end on the minute");
        }

        if (hotOpRequest.UserId is not null && hotOpRequest.UserId != discordUser.Id && !IsAdmin)
        {
            logger.LogWarning("User {UserId} cannot modify hot op {HotOpId}", discordUser.Id, id);
            return Forbid();
        }

        var owner = await GetOwner(hotOpRequest, discordUser, cancellationToken);
        if (owner is null)
        {
            logger.LogWarning("Unable to find user {UserId}", discordUser.Id);
            return BadRequest("Invalid user");
        }
        
        var channel = await uow.Channels.GetById(hotOpRequest.ChannelId, cancellationToken);
        if (channel == null)
        {
            logger.LogWarning("Channel not found");
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
        if (hotOpRequest.UserId is not null)
        {
            hotOpModel.DiscordUser = owner;
            hotOpModel.UserId = owner.Id;
        }
        await uow.CompleteAsync(cancellationToken);
        var hotOpResponse = AdjustDateTimes(hotOpModel.ToHotOpResponse(), discordUser);
        return Ok(hotOpResponse);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHotOp(int id, CancellationToken cancellationToken = default)
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<DiscordUserModel>();
        var hotOpModel = await uow.HotOps.GetById(id, cancellationToken);
        if (hotOpModel == null)
        {
            logger.LogWarning("Hot Op {HotOpId} not found", id);
            return NotFound("Hot Op not found");
        }

        if (hotOpModel.UserId != discordUser.Id && !IsAdmin)
        {
            logger.LogWarning("User {UserId} cannot modify hot op", discordUser.Id);
            return Forbid();
        }

        uow.HotOps.Remove(hotOpModel);
        await uow.CompleteAsync(cancellationToken);
        return Ok();
    }

    [HttpGet("{id:int}/scoreboard")]
    public async Task<ActionResult<IEnumerable<ScoreboardItemResponse>>> GetHotOpScoreboard(int id, CancellationToken cancellationToken = default)
    {
        var hotOp = await uow.HotOps.GetById(id, cancellationToken);
        if (hotOp == null)
        {
            logger.LogWarning("Hot Op {HotOpId} not found", id);
            return NotFound();
        }

        if (hotOp.StartDate > DateTimeOffset.UtcNow)
        {
            logger.LogWarning("Hot Op {HotOpId} has not started yet", id);
            return BadRequest("Hot Op hasn't started yet");
        }

        var scoreboard = hotOpService.GetScoreboard(hotOp);
        var scoreboardResponse = scoreboard.ToScoreboardResponse();
        return Ok(scoreboardResponse);
    }

    private async Task<DiscordUserModel?> GetOwner(
        HotOpRequest hotOpRequest,
        DiscordUserModel callingUser,
        CancellationToken cancellationToken)
    {
        if (hotOpRequest.UserId is null || hotOpRequest.UserId == callingUser.Id)
        {
            return callingUser;
        }
        var owner = await uow.Users.GetById(hotOpRequest.UserId.Value, cancellationToken);
        return owner is { Archived: false } ? owner : null;
    }

    private HotOpResponse AdjustDateTimes(HotOpResponse hotOpResponse, DiscordUserModel discordUser)
    {
        if (!string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            hotOpResponse.StartDate = hotOpResponse.StartDate.AdjustToUsersTimezone(discordUser.Timezone);
            hotOpResponse.EndDate = hotOpResponse.EndDate.AdjustToUsersTimezone(discordUser.Timezone);
        }
        return hotOpResponse;
    }
}