using Brobot.Shared.Responses;
using Brobot.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Brobot.Services;
using Brobot.Repositories;
using Microsoft.AspNetCore.WebUtilities;
using System.Web;
using System.Security.Claims;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _uow;
    private readonly DiscordOauthService _discordOauthService;
    private const string RefreshTokenCookieKey = "refreshCookie";

    public AuthController(
        UserManager<IdentityUser> userManager,
        JwtService jwtService,
        IConfiguration configuration,
        IUnitOfWork uow,
        DiscordOauthService discordOauthService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _configuration = configuration;
        _uow = uow;
        _discordOauthService = discordOauthService;
    }

    [HttpPost("register")]
    public async Task<RegisterResponse> Register(RegisterRequest request)
    {
        var user = new IdentityUser
        {
            Email = request.EmailAddress,
            UserName = request.DisplayName
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }
        return new RegisterResponse
        {
            Succeeded = result.Succeeded,
            Errors = result.Errors.Select((e) => e.Description)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return BadRequest(new LoginResponse
            {
                Succeeded = false,
                Errors = new[] { "Invalid credentials" }
            });
        }

        var result = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!result)
        {
            return BadRequest(new LoginResponse
            {
                Succeeded = false,
                Errors = new[] { "Invalid credentials" }
            });
        }

        var roles = await _userManager.GetRolesAsync(user);

        var discordUser = await _uow.Users.GetFromIdentityUserId(user.Id);
        string jwt = _jwtService.CreateJwt(user, roles.FirstOrDefault(), discordUser?.Id);
        if (!string.IsNullOrWhiteSpace(user.SecurityStamp))
        {
            if (!int.TryParse(_configuration["JwtExpiry"], out var expiry))
            {
                expiry = 30;
            }
            var options = new CookieOptions();
            options.Secure = true;
            options.SameSite = SameSiteMode.Strict;
            options.Expires = DateTime.Now.AddMinutes(expiry);
            HttpContext.Response.Cookies.Append(RefreshTokenCookieKey, user.SecurityStamp, options);
        }

        return Ok(new LoginResponse
        {
            Succeeded = true,
            Token = jwt
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Response.Cookies.Delete(RefreshTokenCookieKey);
        return Ok();
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginResponse>> RefreshToken()
    {
        var cookie = HttpContext.Request.Cookies[RefreshTokenCookieKey];
        if (cookie != null)
        {
            var user = _userManager.Users.FirstOrDefault((user) => user.SecurityStamp == cookie);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var discordUser = await _uow.Users.GetFromIdentityUserId(user.Id);
                var jwtToken = _jwtService.CreateJwt(user, roles.FirstOrDefault(), discordUser?.Id);
                return Ok(new LoginResponse
                {
                    Succeeded = true,
                    Token = jwtToken
                });
            }
        }

        return Ok(new LoginResponse
        {
            Succeeded = false
        });
    }

    [HttpGet("discord-auth")]
    [Authorize]
    public ActionResult DiscordAuth()
    {
        var queryString = new Dictionary<string, string?>
        {
            { "response_type", "code" },
            { "client_id", _configuration["DiscordClientId"] ?? "" },
            { "scope", "identify" },
            { "callback_url", HttpUtility.UrlEncode($"{HttpContext.Request.Host.ToUriComponent()}/discord-cb") }
        };

        var uri = new UriBuilder(QueryHelpers.AddQueryString(_configuration["DiscordAuthorizationEndpoint"] ?? "", queryString));
        return Ok(new { url = uri.ToString() });
    }

    [HttpPost("sync-discord-user")]
    [Authorize]
    public async Task<ActionResult> SyncDiscordUser(SyncDiscordUserRequest syncDiscordUserRequest)
    {
        var nameIdentifierClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (nameIdentifierClaim == null || string.IsNullOrWhiteSpace(nameIdentifierClaim.Value))
        {
            return BadRequest("Unknown user");
        }

        var token = await _discordOauthService.GetToken(syncDiscordUserRequest.AuthorizationCode);
        var id = await _discordOauthService.GetDiscordUserId(token);
        var user = await _uow.Users.GetById(id);
        if (user == null)
        {
            return BadRequest("Unknown user");
        }

        user.IdentityUserId = nameIdentifierClaim.Value;
        await _uow.CompleteAsync();
        return Ok();
    }
}
