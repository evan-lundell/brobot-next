using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class WordCloudController(IUnitOfWork uow, IServiceProvider serviceProvider)  : ControllerBase
{
    [HttpPost("generate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GenerateWordCloud(GenerateWordCloudRequest request)
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
        
        _ = Task.Run(async () =>
        {
            using var scope = serviceProvider.CreateScope();
            var statsService = scope.ServiceProvider.GetRequiredService<IStatsService>();
            var stats = await statsService.GetStats(channel, request.StartDate, request.EndDate);
            await statsService.SendStats(channel.Id, stats);
        });
        return Ok();
    }
}