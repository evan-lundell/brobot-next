using System.Security.Claims;
using System.Web;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Brobot.Mappers;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    UserManager<IdentityUser> userManager,
    JwtService jwtService,
    IConfiguration configuration,
    IUnitOfWork uow,
    DiscordOauthService discordOauthService)
    : ControllerBase
{
    private const string RefreshTokenCookieKey = "refreshCookie";

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest("Passwords don't match");
        }
        var user = new IdentityUser
        {
            Email = request.EmailAddress,
            UserName = request.EmailAddress
        };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new RegisterResponse
            {
                Succeeded = false,
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        await userManager.AddToRoleAsync(user, "User");
        return Ok(new RegisterResponse
        {
            Succeeded = result.Succeeded,
            Errors = result.Errors.Select(e => e.Description)
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return BadRequest(new LoginResponse
            {
                Succeeded = false,
                Errors = ["Invalid credentials"]
            });
        }

        var result = await userManager.CheckPasswordAsync(user, request.Password);
        if (!result)
        {
            return BadRequest(new LoginResponse
            {
                Succeeded = false,
                Errors = ["Invalid credentials"]
            });
        }

        var roles = await userManager.GetRolesAsync(user);

        var discordUser = await uow.Users.GetFromIdentityUserId(user.Id);
        var jwt = jwtService.CreateJwt(user, discordUser, roles.FirstOrDefault());
        // ReSharper disable once InvertIf
        if (!string.IsNullOrWhiteSpace(user.SecurityStamp))
        {
            if (!int.TryParse(configuration["JwtExpiry"], out var expiry))
            {
                expiry = 30;
            }
            var options = new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddMinutes(expiry)
            };
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
        if (cookie == null)
        {
            return Ok(new LoginResponse
            {
                Succeeded = false
            });
        }

        var user = userManager.Users.FirstOrDefault(user => user.SecurityStamp == cookie);
        if (user == null)
        {
            return Ok(new LoginResponse
            {
                Succeeded = false
            });
        }

        var roles = await userManager.GetRolesAsync(user);
        var discordUser = await uow.Users.GetFromIdentityUserId(user.Id);
        var jwtToken = jwtService.CreateJwt(user, discordUser, roles.FirstOrDefault());
        return Ok(new LoginResponse
        {
            Succeeded = true,
            Token = jwtToken
        });

    }

    [HttpGet("discord-auth")]
    [Authorize]
    public ActionResult DiscordAuth()
    {
        var queryString = new Dictionary<string, string?>
        {
            { "response_type", "code" },
            { "client_id", configuration["DiscordClientId"] ?? "" },
            { "scope", "identify" },
            { "callback_url", HttpUtility.UrlEncode($"{HttpContext.Request.Host.ToUriComponent()}/discord-cb") }
        };

        var uri = new UriBuilder(QueryHelpers.AddQueryString(configuration["DiscordAuthorizationEndpoint"] ?? "", queryString));
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

        var token = await discordOauthService.GetToken(syncDiscordUserRequest.AuthorizationCode);
        var id = await discordOauthService.GetDiscordUserId(token);
        var user = await uow.Users.GetById(id);
        if (user == null)
        {
            return BadRequest("Unknown user");
        }

        user.IdentityUserId = nameIdentifierClaim.Value;
        await uow.CompleteAsync();
        return Ok();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid password");
        }
        
        var identityUser = await userManager.GetUserAsync(HttpContext.User);
        if (identityUser == null)
        {
            return Unauthorized();
        }

        var result = await userManager.ChangePasswordAsync(identityUser, changePasswordRequest.CurrentPassword,
            changePasswordRequest.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.FirstOrDefault()?.Description ?? "Password update failed");
        }
        
        if (!string.IsNullOrWhiteSpace(identityUser.SecurityStamp))
        {
            if (!int.TryParse(configuration["JwtExpiry"], out var expiry))
            {
                expiry = 30;
            }
            var options = new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddMinutes(expiry)
            };
            HttpContext.Response.Cookies.Append(RefreshTokenCookieKey, identityUser.SecurityStamp, options);
        }

        return Ok();
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<IdentityUserResponse>>> GetIdentityUsers()
    {
        var identityUsers = await userManager.Users.ToListAsync();
        var identityUserResponses = identityUsers.Select(iu => iu.ToIdentityUserResponse()).ToList();
        var discordUsers = await uow.Users
            .GetUsersFromIdentityUserIds(identityUsers.Select(iu => iu.Id));
        foreach (var identityUserResponse in identityUserResponses)
        {
            identityUserResponse.IsDiscordAuthenticated =
                // ReSharper disable once PossibleMultipleEnumeration
                discordUsers.Any(du => du.IdentityUserId == identityUserResponse.Id);
        }
        
        return Ok(identityUserResponses);
    }
}
