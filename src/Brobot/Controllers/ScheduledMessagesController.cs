using AutoMapper;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ScheduledMessagesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ScheduledMessageService _scheduledMessageService;

    public ScheduledMessagesController(IUnitOfWork uow, IMapper mapper, ScheduledMessageService scheduledMessageService)
    {
        _uow = uow;
        _mapper = mapper;
        _scheduledMessageService = scheduledMessageService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduledMessageResponse>>> Get(
        [FromQuery] int limit = 10,
        [FromQuery] int skip = 0,
        [FromQuery] DateTime? scheduledBefore = null,
        [FromQuery] DateTime? scheduledAfter = null)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        if (limit > 50)
        {
            return BadRequest("Limit cannot be greater than 50");
        }

        var scheduledMessages = await _uow.ScheduledMessages.GetScheduledMessagesByUser(discordUser.Id, limit, skip, scheduledBefore, scheduledAfter);
        return Ok(_mapper.Map<IEnumerable<ScheduledMessageResponse>>(scheduledMessages));
    }

    [HttpPost]
    public async Task<ActionResult<ScheduledMessageResponse>> CreateScheduledMessage(ScheduledMessageRequest scheduledMessage)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        if (scheduledMessage.ChannelId == null)
        {
            return BadRequest("Channel Id not given");
        }

        var channelModel = await _uow.Channels.GetById(scheduledMessage.ChannelId.Value);

        if (channelModel == null)
        {
            return BadRequest("Bad channel ID supplied");
        }

        var newScheduledMessage = await _scheduledMessageService.CreateScheduledMessage(scheduledMessage.MessageText,
            discordUser, scheduledMessage.SendDate, channelModel);
        return Ok(_mapper.Map<ScheduledMessageResponse>(newScheduledMessage));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ScheduledMessageResponse>> UpdateScheduledMessage(int id, ScheduledMessageRequest scheduledMessageRequest)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        var scheduledMessage = await _uow.ScheduledMessages.GetById(id);
        if (scheduledMessage == null)
        {
            return NotFound("Scheduled Message not found");
        }

        if (scheduledMessage.CreatedById != discordUser.Id)
        {
            return Unauthorized();
        }

        if (scheduledMessageRequest.ChannelId == null)
        {
            return BadRequest("Bad channel ID");
        }
        var channel = await _uow.Channels.GetById(scheduledMessageRequest.ChannelId.Value);
        if (channel == null)
        {
            return BadRequest(("Bad channel ID"));
        }
        
        scheduledMessage.MessageText = scheduledMessageRequest.MessageText;
        scheduledMessage.SendDate = scheduledMessageRequest.SendDate;
        scheduledMessage.ChannelId = channel.Id;
        scheduledMessage.Channel = channel;
        await _uow.CompleteAsync();
        return Ok(_mapper.Map<ScheduledMessageResponse>(scheduledMessage));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScheduledMessage(int id)
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }
        
        var scheduledMessage = await _uow.ScheduledMessages.GetById(id);
        if (scheduledMessage == null)
        {
            return NotFound("Scheduled Message not found");
        }

        if (scheduledMessage.CreatedById != discordUser.Id)
        {
            return Unauthorized();
        }

        _uow.ScheduledMessages.Remove(scheduledMessage);
        await _uow.CompleteAsync();
        return Ok();
    }
}