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
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class StopWordsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly StopWordService _stopWordService;

    public StopWordsController(IUnitOfWork uow, IMapper mapper, StopWordService stopWordService)
    {
        _uow = uow;
        _mapper = mapper;
        _stopWordService = stopWordService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StopWordResponse>>> GetAll()
    {
        var stopWordModels = await _uow.StopWords.GetAll();
        return Ok(_mapper.Map<IEnumerable<StopWordResponse>>(stopWordModels));
    }

    [HttpPost]
    public async Task<ActionResult<StopWordResponse>> CreateStopWord(StopWordRequest stopWordRequest)
    {
        if (await _uow.StopWords.StopWordExists(stopWordRequest.Word))
        {
            return BadRequest("Stop word already exists");
        }

        var stopWordModel = _mapper.Map<StopWordModel>(stopWordRequest);
        await _uow.StopWords.Add(stopWordModel);
        await _uow.CompleteAsync();
        _stopWordService.StopWordsUpdated();
        return Ok(_mapper.Map<StopWordResponse>(stopWordModel));
    }

    [HttpDelete("{word}")]
    public async Task<ActionResult> DeleteStopWord(string word)
    {
        var stopWordModel = await _uow.StopWords.GetByWord(word);
        if (stopWordModel == null)
        {
            return BadRequest("Stop word doesn't exist");
        }
        
        _uow.StopWords.Remove(stopWordModel);
        await _uow.CompleteAsync();
        _stopWordService.StopWordsUpdated();
        return Ok();
    }
}