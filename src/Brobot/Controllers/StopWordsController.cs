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
public class StopWordsController(IUnitOfWork uow, IStopWordService stopWordService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StopWordResponse>>> GetAll()
    {
        var stopWordModels = await uow.StopWords.GetAll();
        return Ok(stopWordModels.Select(model => model.ToStopWordResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<StopWordResponse>> CreateStopWord(StopWordRequest stopWordRequest)
    {
        stopWordRequest.Word = stopWordRequest.Word.ToLower();
        if (await uow.StopWords.StopWordExists(stopWordRequest.Word))
        {
            return BadRequest("Stop word already exists");
        }

        var stopWordModel = stopWordRequest.ToStopWordModel();
        await uow.StopWords.Add(stopWordModel);
        await uow.CompleteAsync();
        stopWordService.StopWordsUpdated();
        return Ok(stopWordModel.ToStopWordResponse());
    }

    [HttpDelete("{word}")]
    public async Task<ActionResult> DeleteStopWord(string word)
    {
        var stopWordModel = await uow.StopWords.GetByWord(word);
        if (stopWordModel == null)
        {
            return BadRequest("Stop word doesn't exist");
        }
        
        uow.StopWords.Remove(stopWordModel);
        await uow.CompleteAsync();
        stopWordService.StopWordsUpdated();
        return Ok();
    }
}