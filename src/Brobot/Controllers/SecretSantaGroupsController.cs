using Brobot.Exceptions;
using Brobot.Services;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SecretSantaGroupsController : ControllerBase
{
    private readonly SecretSantaService _secretSantaService;

    public SecretSantaGroupsController(SecretSantaService secretSantaService)
    {
        _secretSantaService = secretSantaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SecretSantaGroupResponse>>> GetSecretSantaGroups()
    {
        var secretSantaGroupResponses = await _secretSantaService.GetSecretSantaGroups();
        return Ok(secretSantaGroupResponses);
    }

    [HttpGet("{secretSantaGroupId}")]
    public async Task<ActionResult<SecretSantaGroupResponse>> GetSecretSantaGroup(int secretSantaGroupId)
    {
        var secretSantaGroup = await _secretSantaService.GetSecretSantaGroup(secretSantaGroupId);
        if (secretSantaGroup == null)
        {
            return NotFound("Secret santa group not found");
        }

        return Ok(secretSantaGroup);
    }

    [HttpPost]
    public async Task<ActionResult<SecretSantaGroupResponse>> CreateSecretSantaGroup([FromBody] SecretSantaGroupRequest secretSanta)
    {
        try
        {
            if (secretSanta.Users.Count == 0)
            {
                return BadRequest("No users specified.");
            }

            var secretSantaResponse = await _secretSantaService.CreateSecretSantaGroup(secretSanta);
            return Ok(secretSantaResponse);
        }
        catch (ModelNotFoundException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("{secretSantaGroupId}/members")]
    public async Task<ActionResult<SecretSantaGroupResponse>> AddUserToGroup(int secretSantaGroupId, UserResponse user)
    {
        try
        {
            var secretSantaGroupResponse = await _secretSantaService.AddUserToGroup(secretSantaGroupId, user);
            return Ok(secretSantaGroupResponse);
        }
        catch (ModelNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpDelete("{secretSantaGroupId}/members/{userId}")]
    public async Task<ActionResult<SecretSantaGroupResponse>> RemoveUserFromGroup(int secretSantaGroupId, ulong userId)
    {
        try
        {
            var secretSantaGroupResponse = await _secretSantaService.RemoveUserFromGroup(secretSantaGroupId, userId);
            return Ok(secretSantaGroupResponse);
        }
        catch (ModelNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost("{secretSantaGroupId}/pairings")]
    public async Task<ActionResult<SecretSantaGroupResponse>> GeneratePairs(int secretSantaGroupId)
    {
        try
        {
            var pairs = await _secretSantaService.GeneratePairsForCurrentYear(secretSantaGroupId);
            await _secretSantaService.SendPairs(pairs);
            return Ok(pairs);
        }
        catch (ModelNotFoundException mnfEx)
        {
            return NotFound(mnfEx.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}