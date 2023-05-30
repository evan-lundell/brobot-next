using System.Security.Claims;
using AutoMapper;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMapper _mapper;

    public UsersController(
        IUnitOfWork uow,
        UserManager<IdentityUser> userManager,
        IMapper mapper
    )
    {
        _uow = uow;
        _userManager = userManager;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize]
    public ActionResult<UserResponse> GetUser()
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

        return Ok(_mapper.Map<UserResponse>(discordUser));
    }

    [HttpGet("settings")]
    [Authorize]
    public ActionResult<UserSettingsResponse> GetUserSettings()
    {
        if (HttpContext.Items["DiscordUser"] is not UserModel discordUser)
        {
            return Unauthorized();
        }

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
        var identityUser = await _userManager.GetUserAsync(HttpContext.User);
        if (identityUser == null)
        {
            return Unauthorized();
        }

        var discordUser = await _uow.Users.GetFromIdentityUserId(identityUser.Id);
        if (discordUser == null)
        {
            return Unauthorized();
        }

        if (userSettingsRequest.BirthDate != null)
        {
            userSettingsRequest.BirthDate = DateTime.SpecifyKind(userSettingsRequest.BirthDate.Value, DateTimeKind.Utc);
        }
        discordUser.Birthdate = userSettingsRequest.BirthDate;
        discordUser.Timezone = userSettingsRequest.Timezone;
        discordUser.PrimaryChannelId = userSettingsRequest.PrimaryChannelId;
        await _uow.CompleteAsync();

        return Ok(new UserSettingsResponse
        {
            Birthdate = discordUser.Birthdate,
            Timezone = discordUser.Timezone,
            PrimaryChannelId = discordUser.PrimaryChannelId
        });
    }
}