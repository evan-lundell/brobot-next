using System.Web;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Brobot.Configuration;
using Brobot.Models;
using Microsoft.Extensions.Options;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    UserManager<ApplicationUserModel> userManager,
    IOptions<JwtOptions> jwtOptions,
    IOptions<DiscordOptions> discordOptions,
    ILogger<AuthController> logger)
    : ControllerBase
{
    private const string RefreshTokenCookieKey = "refreshCookie";

    [HttpPost("login")]
    public Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Response.Cookies.Delete(RefreshTokenCookieKey);
        return Ok();
    }

    [HttpPost("refresh-token")]
    public Task<ActionResult<LoginResponse>> RefreshToken()
    {
        return Task.FromResult<ActionResult<LoginResponse>>(Ok(new LoginResponse { Succeeded = false }));
        
    }

    [HttpGet("discord-auth")]
    [Authorize]
    public ActionResult DiscordAuth()
    {
        var queryString = new Dictionary<string, string?>
        {
            { "response_type", "code" },
            { "client_id", discordOptions.Value.ClientId },
            { "scope", "identify" },
            { "callback_url", HttpUtility.UrlEncode($"{HttpContext.Request.Host.ToUriComponent()}/discord-cb") }
        };

        var uri = new UriBuilder(QueryHelpers.AddQueryString(discordOptions.Value.AuthorizationEndpoint, queryString));
        return Ok(new { url = uri.ToString() });
    }
    
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid password change request");
            return BadRequest("Invalid password");
        }
        
        var identityUser = await userManager.GetUserAsync(HttpContext.User);
        if (identityUser == null)
        {
            logger.LogWarning("Cannot find user by identity user.");
            return Unauthorized();
        }

        var result = await userManager.ChangePasswordAsync(identityUser, changePasswordRequest.CurrentPassword,
            changePasswordRequest.NewPassword);
        if (!result.Succeeded)
        {
            logger.LogWarning("User {UserId} failed to change password. {Error}", identityUser.Id,  result.Errors.FirstOrDefault()?.Description ?? "Password update failed");
            return BadRequest(result.Errors.FirstOrDefault()?.Description ?? "Password update failed");
        }
        
        if (!string.IsNullOrWhiteSpace(identityUser.SecurityStamp))
        {
            var options = new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddMinutes(jwtOptions.Value.Expiry)
            };
            HttpContext.Response.Cookies.Append(RefreshTokenCookieKey, identityUser.SecurityStamp, options);
        }

        return Ok();
    }
}
