using Brobot.Services;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SecretSantaGroupsController(ISecretSantaService secretSantaService, ILogger<SecretSantaGroupsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SecretSantaGroupResponse>>> GetSecretSantaGroups(CancellationToken cancellationToken = default)
    {
        var secretSantaGroupResponses = await secretSantaService.GetSecretSantaGroups(cancellationToken);
        return Ok(secretSantaGroupResponses);
    }

    [HttpGet("{secretSantaGroupId}")]
    public async Task<ActionResult<SecretSantaGroupResponse>> GetSecretSantaGroup(int secretSantaGroupId, CancellationToken cancellationToken = default)
    {
        var secretSantaGroup = await secretSantaService.GetSecretSantaGroup(secretSantaGroupId, cancellationToken);
        if (secretSantaGroup == null)
        {
            logger.LogWarning("Secret Santa Group {SecretSantaGroupId} not found.", secretSantaGroupId);
            return NotFound("Secret santa group not found");
        }

        return Ok(secretSantaGroup);
    }

    [HttpPost]
    public async Task<ActionResult<SecretSantaGroupResponse>> CreateSecretSantaGroup(
        [FromBody] SecretSantaGroupRequest secretSanta,
        CancellationToken cancellationToken = default)
    {
        if (secretSanta.Users.Count == 0)
        {
            logger.LogWarning("No users specified.");
            return BadRequest("No users specified.");
        }

        var secretSantaResponse = await secretSantaService.CreateSecretSantaGroup(secretSanta, cancellationToken);
        return Ok(secretSantaResponse);

    }

    [HttpPost("{secretSantaGroupId}/members")]
    public async Task<ActionResult<SecretSantaGroupResponse>> AddUserToGroup(int secretSantaGroupId, DiscordUserResponse discordUser, CancellationToken cancellationToken = default)
    {
        var secretSantaGroupResponse = await secretSantaService.AddUserToGroup(secretSantaGroupId, discordUser, cancellationToken);
        return Ok(secretSantaGroupResponse);
    }

    [HttpDelete("{secretSantaGroupId}/members/{userId}")]
    public async Task<ActionResult<SecretSantaGroupResponse>> RemoveUserFromGroup(int secretSantaGroupId, ulong userId, CancellationToken cancellationToken = default)
    {
        var secretSantaGroupResponse = await secretSantaService.RemoveUserFromGroup(secretSantaGroupId, userId, cancellationToken);
        return Ok(secretSantaGroupResponse);
    }

    [HttpPost("{secretSantaGroupId}/pairings")]
    public async Task<ActionResult<SecretSantaGroupResponse>> GeneratePairs(int secretSantaGroupId, CancellationToken cancellationToken = default)
    {
        var pairs = await secretSantaService.GeneratePairsForCurrentYear(secretSantaGroupId, cancellationToken);
        await secretSantaService.SendPairs(pairs, cancellationToken);
        return Ok(pairs);
    }
}
