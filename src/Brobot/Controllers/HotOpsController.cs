using AutoMapper;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HotOpsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly HotOpService _hotOpService;
    private readonly IMapper _mapper;

    public HotOpsController(
        IUnitOfWork uow, 
        HotOpService hotOpService,
        IMapper mapper)
    {
        _uow = uow;
        _hotOpService = hotOpService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HotOpResponse>>> GetHotOps([FromQuery] HotOpQueryType type = HotOpQueryType.All)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        IEnumerable<HotOpModel>? hotOpModels;
        switch (type)
        {
            case HotOpQueryType.Upcoming:
                var now = DateTimeOffset.UtcNow;
                 hotOpModels = await _uow.HotOps.Find((ho) => ho.UserId == discordUser.Id && ho.StartDate > now);
                break;
            case HotOpQueryType.Current:
                hotOpModels = await _uow.HotOps.GetUsersHotOps(discordUser.Id, HotOpQueryType.Current);
                break;
            case HotOpQueryType.Completed:
                hotOpModels = await _uow.HotOps.GetUsersHotOps(discordUser.Id, HotOpQueryType.Completed);
                break;
            default:
                hotOpModels = await _uow.HotOps.Find((ho) => ho.UserId == discordUser.Id);
                break;
        }

        var hotOps = _mapper.Map<IEnumerable<HotOpResponse>>(hotOpModels);
        if (!string.IsNullOrWhiteSpace(discordUser.Timezone))
        {
            foreach (var hotOp in hotOps)
            {
                hotOp.StartDate = hotOp.StartDate.AdjustToUsersTimezone(discordUser.Timezone);
                hotOp.EndDate = hotOp.EndDate.AdjustToUsersTimezone(discordUser.Timezone);
            }
        }
        
        return Ok(hotOps ?? Array.Empty<HotOpResponse>());
    }
    
    [HttpPost]
    public async Task<ActionResult<HotOpResponse>> CreateHotOp(HotOpRequest hotOpRequest)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }
        
        if (hotOpRequest.StartDate >= hotOpRequest.EndDate)
        {
            return BadRequest("Start date must be before end date");
        }
        
        var channel = await _uow.Channels.GetById(hotOpRequest.ChannelId);
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
        var hotOpModel = new HotOpModel
        {
            UserId = discordUser.Id,
            User = discordUser,
            Channel = channel,
            ChannelId = channel.Id,
            StartDate = adjustedTimes.StartDate,
            EndDate = adjustedTimes.EndDate
        };

        await _uow.HotOps.Add(hotOpModel);
        await _uow.CompleteAsync();
        var hotOpResponse = _mapper.Map<HotOpResponse>(hotOpModel);
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
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var hotOpModel = await _uow.HotOps.GetById(id);
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
        
        var channel = await _uow.Channels.GetById(hotOpRequest.ChannelId);
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
        await _uow.CompleteAsync();
        var hotOpResponse = _mapper.Map<HotOpResponse>(hotOpModel);
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
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var hotOpModel = await _uow.HotOps.GetById(id);
        if (hotOpModel == null)
        {
            return NotFound("Hot Op not found");
        }

        if (hotOpModel.UserId != discordUser.Id)
        {
            return Unauthorized();
        }

        _uow.HotOps.Remove(hotOpModel);
        await _uow.CompleteAsync();
        return Ok();
    }

    [HttpGet("{id:int}/scoreboard")]
    public async Task<ActionResult<IEnumerable<ScoreboardItemResponse>>> GetHotOpScoreboard(int id)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel)
        {
            return Unauthorized();
        }

        var hotOp = await _uow.HotOps.GetById(id);
        if (hotOp == null)
        {
            return NotFound();
        }

        if (hotOp.StartDate > DateTimeOffset.UtcNow)
        {
            return BadRequest("Hot Op hasn't started yet");
        }

        var scoreboard = _hotOpService.GetScoreboard(hotOp);
        var scoreboardResponse = _mapper.Map<ScoreboardResponse>(scoreboard); 
        return Ok(scoreboardResponse);
    }
}