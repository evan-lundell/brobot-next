using Brobot.Dtos;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Services;

public class AuthService(
    UserManager<ApplicationUserModel> userManager,
    IUnitOfWork uow,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResultDto> GetOrCreateApplicationUserAsync(ulong discordUserId)
    {
        // Check if this Discord user exists in our database
        var discordUser = await uow.Users.GetById(discordUserId);
        if (discordUser == null)
        {
            logger.LogWarning("Discord user {DiscordUserId} attempted to login but does not exist in database", discordUserId);
            return new AuthResultDto(
                Succeeded: false,
                ErrorMessage: "You are not authorized to access this application. Please contact an administrator.");
        }

        // Check if an ApplicationUser already exists for this Discord user
        var existingUser = await userManager.Users
            .FirstOrDefaultAsync(u => u.DiscordUserId == discordUserId);
        ApplicationUserModel applicationUser;

        if (existingUser == null)
        {
            // Create a new ApplicationUser linked to this Discord user
            applicationUser = new ApplicationUserModel
            {
                UserName = discordUser.Username,
                DiscordUserId = discordUserId,
                DiscordUser = discordUser
            };

            var createResult = await userManager.CreateAsync(applicationUser);
            if (!createResult.Succeeded)
            {
                logger.LogError("Failed to create ApplicationUser for Discord user {DiscordUserId}: {Errors}",
                    discordUserId,
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return new AuthResultDto(
                    Succeeded: false,
                    ErrorMessage: "Failed to create user account. Please try again.");
            }

            var roleResult = await userManager.AddToRoleAsync(applicationUser, Constants.UserRoleName);
            if (!roleResult.Succeeded)
            {
                logger.LogError("Failed to assign role to ApplicationUser {UserId}: {Errors}",
                    applicationUser.Id,
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                return new AuthResultDto(
                    Succeeded: false,
                    ErrorMessage: "Failed to assign user role. Please contact an administrator.");
            }

            logger.LogInformation("Created new ApplicationUser for Discord user {DiscordUserId}", discordUserId);
        }
        else
        {
            applicationUser = existingUser;
        }

        var roles = await userManager.GetRolesAsync(applicationUser);

        if (roles.Count == 0)
        {
            logger.LogWarning("ApplicationUser {UserId} has no roles assigned", applicationUser.Id);
            return new AuthResultDto(
                Succeeded: false,
                ErrorMessage: "You do not have any roles assigned. Please contact an administrator.");
        }

        return new AuthResultDto(
            Succeeded: true,
            User: applicationUser,
            DiscordUser: discordUser,
            Roles: roles);
    }
}

