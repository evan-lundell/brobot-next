using AutoMapper;
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
        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
        return Ok(_mapper.Map<UserResponse>(discordUser));
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
        var identityUser = await _userManager.GetUserAsync(HttpContext.User);
        if (identityUser == null)
        {
            return Unauthorized();
        }

        var discordUser = HttpContext.Features.GetRequiredFeature<UserModel>();
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

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserModel>>> GetAllUsers()
    {
        var users = await _uow.Users.GetAll();
        return Ok(_mapper.Map<UserModel[]>(users));
    }

}