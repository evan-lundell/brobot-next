using Brobot.Mappers;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController(
    IUnitOfWork uow,
    UserManager<IdentityUser> userManager,
    ILogger<UsersController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public ActionResult<UserResponse> GetUser()
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        return Ok(discordUser.ToUserResponse());
    }

    [HttpGet("settings")]
    [Authorize]
    public ActionResult<UserSettingsResponse> GetUserSettings()
    {
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        var settings = new UserSettingsResponse
        {
            Birthdate = discordUser.Birthdate,
            Timezone = discordUser.Timezone,
            PrimaryChannelId = discordUser.PrimaryChannelId
        };
        return Ok(settings);
    }

    [HttpPatch("settings")]
    [Authorize]
    public async Task<ActionResult<UserSettingsResponse>> UpdateUserSettings(UserSettingsRequest userSettingsRequest)
    {
        var identityUser = await userManager.GetUserAsync(HttpContext.User);
        if (identityUser == null)
        {
            logger.LogWarning("Identity user not found");
            return Unauthorized();
        }

        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        discordUser.Birthdate = userSettingsRequest.BirthDate;
        discordUser.Timezone = userSettingsRequest.Timezone;
        discordUser.PrimaryChannelId = userSettingsRequest.PrimaryChannelId;
        await uow.CompleteAsync();

        return Ok(new UserSettingsResponse
        {
            Birthdate = discordUser.Birthdate,
            Timezone = discordUser.Timezone,
            PrimaryChannelId = discordUser.PrimaryChannelId
        });
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserModel>>> GetAllUsers()
    {
        var users = await uow.Users.GetAll();
        return Ok(users.Select(u => u.ToUserResponse()));
    }
}
