using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class WordCloudController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly WordCloudService _wordCloudService;

    public WordCloudController(IUnitOfWork uow, WordCloudService wordCloudService)
    {
        _uow = uow;
        _wordCloudService = wordCloudService;
    }
    
    [HttpGet]
    public async Task<ActionResult<MonthlyWordCloudResponse>> GetWordCloud([FromQuery] ulong channelId, [FromQuery] int monthsBack = 1)
    {
        var channelModel = await _uow.Channels.GetById(channelId);
        if (channelModel == null)
        {
            return NotFound();
        }

        var result = await _wordCloudService.GenerateWordCloud(channelModel, monthsBack);
        return Ok(result);
    }
}