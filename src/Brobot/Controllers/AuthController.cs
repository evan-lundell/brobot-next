using System.Security.Cryptography;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Brobot.Configuration;
using Brobot.Models;
using Brobot.Services;
using Microsoft.Extensions.Options;

namespace Brobot.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    UserManager<ApplicationUserModel> userManager,
    IOptions<DiscordOptions> discordOptions,
    DiscordOauthService discordOauthService,
    IJwtService jwtService,
    IAuthService authService,
    ILogger<AuthController> logger)
    : ControllerBase
{
    private const string RefreshTokenCookieKey = "refreshCookie";
    private const string OAuthStateCookieKey = "oauth_state";

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Response.Cookies.Delete(RefreshTokenCookieKey);
        return Ok();
    }

    [HttpPost("discord-login")]
    public async Task<ActionResult<LoginResponse>> DiscordLogin(DiscordLoginRequest request)
    {
        try
        {
            // Validate the state parameter to prevent CSRF attacks
            if (!HttpContext.Request.Cookies.TryGetValue(OAuthStateCookieKey, out var storedState) ||
                string.IsNullOrEmpty(storedState) ||
                storedState != request.State)
            {
                logger.LogWarning("OAuth state mismatch - possible CSRF attack");
                return Ok(new LoginResponse
                {
                    Succeeded = false,
                    Errors = ["Invalid authentication state. Please try again."]
                });
            }
            
            // Clear the state cookie after validation
            HttpContext.Response.Cookies.Delete(OAuthStateCookieKey);
            
            // Exchange the authorization code for an access token
            var accessToken = await discordOauthService.GetToken(request.AuthorizationCode, request.RedirectUri);
            
            // Get the Discord user ID from the access token
            var discordUserId = await discordOauthService.GetDiscordUserId(accessToken);
            
            // Get or create the application user
            var authResult = await authService.GetOrCreateApplicationUserAsync(discordUserId);
            if (!authResult.Succeeded)
            {
                return Ok(new LoginResponse
                {
                    Succeeded = false,
                    Errors = [authResult.ErrorMessage!]
                });
            }
            
            // Generate JWT token
            var token = jwtService.CreateJwt(authResult.User!, authResult.DiscordUser!, authResult.Roles!);
            
            // Set refresh token cookie (longer expiry than JWT)
            if (!string.IsNullOrWhiteSpace(authResult.User!.SecurityStamp))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                HttpContext.Response.Cookies.Append(RefreshTokenCookieKey, authResult.User.SecurityStamp, cookieOptions);
            }

            logger.LogInformation("Discord user {DiscordUserId} logged in successfully", discordUserId);
            return Ok(new LoginResponse
            {
                Succeeded = true,
                Token = token
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Discord login");
            return Ok(new LoginResponse
            {
                Succeeded = false,
                Errors = ["An error occurred during login. Please try again."]
            });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginResponse>> RefreshToken()
    {
        try
        {
            // Get the refresh token from the cookie
            if (!HttpContext.Request.Cookies.TryGetValue(RefreshTokenCookieKey, out var refreshToken) ||
                string.IsNullOrEmpty(refreshToken))
            {
                logger.LogDebug("No refresh token cookie found");
                return Ok(new LoginResponse { Succeeded = false });
            }

            // Find the user by their SecurityStamp (refresh token)
            var applicationUser = await userManager.Users
                .Include(u => u.DiscordUser)
                .FirstOrDefaultAsync(u => u.SecurityStamp == refreshToken);

            if (applicationUser == null)
            {
                logger.LogWarning("Invalid refresh token - no user found");
                HttpContext.Response.Cookies.Delete(RefreshTokenCookieKey);
                return Ok(new LoginResponse { Succeeded = false });
            }

            var discordUser = applicationUser.DiscordUser;
            var roles = await userManager.GetRolesAsync(applicationUser);
            
            if (roles.Count == 0)
            {
                logger.LogWarning("ApplicationUser {UserId} has no roles assigned", applicationUser.Id);
                return Ok(new LoginResponse
                {
                    Succeeded = false,
                    Errors = ["You do not have any roles assigned. Please contact an administrator."]
                });
            }
            
            // Generate a new JWT token
            var token = jwtService.CreateJwt(applicationUser, discordUser, roles);

            // Rotate the refresh token for security (generate new SecurityStamp)
            await userManager.UpdateSecurityStampAsync(applicationUser);
            var newSecurityStamp = await userManager.GetSecurityStampAsync(applicationUser);

            // Set the new refresh token cookie
            if (!string.IsNullOrWhiteSpace(newSecurityStamp))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7) // Refresh tokens last longer than JWTs
                };
                HttpContext.Response.Cookies.Append(RefreshTokenCookieKey, newSecurityStamp, cookieOptions);
            }

            logger.LogInformation("Token refreshed for user {UserId}", applicationUser.Id);
            return Ok(new LoginResponse
            {
                Succeeded = true,
                Token = token
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token refresh");
            return Ok(new LoginResponse { Succeeded = false });
        }
    }

    [HttpGet("discord-auth")]
    public ActionResult DiscordAuth()
    {
        var scheme = HttpContext.Request.Scheme;
        var host = HttpContext.Request.Host.ToUriComponent();
        var redirectUri = $"{scheme}://{host}/discord-cb";
        
        // Generate a cryptographically secure random state value
        var stateBytes = RandomNumberGenerator.GetBytes(32);
        var state = Convert.ToBase64String(stateBytes);
        
        // Store the state in an HttpOnly cookie for validation on callback
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax, // Lax required for OAuth redirect flow
            Expires = DateTime.UtcNow.AddMinutes(10) // Short expiry for security
        };
        HttpContext.Response.Cookies.Append(OAuthStateCookieKey, state, cookieOptions);
        
        var queryString = new Dictionary<string, string?>
        {
            { "response_type", "code" },
            { "client_id", discordOptions.Value.ClientId },
            { "scope", "identify" },
            { "redirect_uri", redirectUri },
            { "state", state }
        };

        var uri = new UriBuilder(QueryHelpers.AddQueryString(discordOptions.Value.AuthorizationEndpoint, queryString));
        return Ok(new { url = uri.ToString() });
    }
}
