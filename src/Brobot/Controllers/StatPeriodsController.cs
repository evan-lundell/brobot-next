using Brobot.Mappers;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared.Requests;
using Brobot.TaskQueue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class StatPeriodsController(IUnitOfWork uow, IBackgroundTaskQueue backgroundTaskQueue, IServiceScopeFactory scopeFactory)  : ControllerBase
{
    [HttpGet("{statPeriodId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetStatPeriod([FromRoute] int statPeriodId)
    {
        var statPeriodModel = await uow.StatPeriods.GetStatPeriodWithCounts(statPeriodId);
        if (statPeriodModel == null)
        {
            return NotFound($"Stat period with id {statPeriodId} not found");
        }
        return Ok(statPeriodModel.ToStatPeriodResponse());
    }
    
    [HttpPost("collect")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CollectStats(StatPeriodRequest request)
    {
        var channel = await uow.Channels.GetById(request.ChannelId);
        if (channel == null)
        {
            return NotFound($"Channel with id {request.ChannelId} not found");
        }

        if (request.EndDate < request.StartDate)
        {
            return BadRequest("End date must be greater than start date");
        }

        StatPeriodModel statPeriod = new()
        {
            Channel = channel,
            ChannelId = channel.Id,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        await uow.StatPeriods.Add(statPeriod);
        await uow.CompleteAsync();
        
        backgroundTaskQueue.QueueBackgroundWorkItem(async _ =>
        {
            using var scope = scopeFactory.CreateScope();
            var statsService = scope.ServiceProvider.GetRequiredService<IStatsService>();
            await statsService.GetStats(channel, request.StartDate, request.EndDate, statPeriod.Id);
        });
        return Ok(statPeriod.ToStatPeriodResponse());
    }
    
    
    [HttpPost("generate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GenerateWordCloud(StatPeriodRequest request)
    {
        var channel = await uow.Channels.GetByIdNoTracking(request.ChannelId);
        if (channel == null)
        {
            return NotFound($"Channel with id {request.ChannelId} not found");
        }

        if (request.EndDate < request.StartDate)
        {
            return BadRequest("End date must be greater than start date");
        }

        StatPeriodModel statPeriod = new()
        {
            Channel = channel,
            ChannelId = channel.Id,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        await uow.StatPeriods.Add(statPeriod);
        await uow.CompleteAsync();
        
        backgroundTaskQueue.QueueBackgroundWorkItem(async _ =>
        {
            using var scope = scopeFactory.CreateScope();
            var statsService = scope.ServiceProvider.GetRequiredService<IStatsService>();
            var stats = await statsService.GetStats(channel, request.StartDate, request.EndDate, statPeriod.Id);
            await statsService.SendStats(channel.Id, stats);
        });
        return Ok(statPeriod.ToStatPeriodResponse());
    }
}