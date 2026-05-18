using Brobot.Mappers;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StopWordsController(
    IUnitOfWork uow,
    IStopWordService stopWordService,
    ILogger<StopWordsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StopWordResponse>>> GetAll(CancellationToken cancellationToken = default)
    {
        var stopWordModels = await uow.StopWords.GetAll(cancellationToken);
        return Ok(stopWordModels.Select(model => model.ToStopWordResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<StopWordResponse>> CreateStopWord(StopWordRequest stopWordRequest, CancellationToken cancellationToken = default)
    {
        stopWordRequest.Word = stopWordRequest.Word.ToLower();
        if (await uow.StopWords.StopWordExists(stopWordRequest.Word, cancellationToken))
        {
            logger.LogWarning("Stop word already exists");
            return BadRequest("Stop word already exists");
        }

        var stopWordModel = stopWordRequest.ToStopWordModel();
        await uow.StopWords.Add(stopWordModel, cancellationToken);
        await uow.CompleteAsync(cancellationToken);
        stopWordService.StopWordsUpdated();
        return Ok(stopWordModel.ToStopWordResponse());
    }

    [HttpDelete("{word}")]
    public async Task<ActionResult> DeleteStopWord(string word, CancellationToken cancellationToken = default)
    {
        var stopWordModel = await uow.StopWords.GetByWord(word, cancellationToken);
        if (stopWordModel == null)
        {
            logger.LogWarning("Stop word not found");
            return BadRequest("Stop word doesn't exist");
        }
        
        uow.StopWords.Remove(stopWordModel);
        await uow.CompleteAsync(cancellationToken);
        stopWordService.StopWordsUpdated();
        return Ok();
    }
}