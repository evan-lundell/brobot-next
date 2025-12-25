using Brobot.Exceptions;
using Brobot.Mappers;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Discord;

namespace Brobot.Services;

public class SecretSantaService(
    IUnitOfWork uow,
    IDiscordClient client,
    Random random,
    ILogger<SecretSantaService> logger) : ISecretSantaService
{
    public async Task<IEnumerable<SecretSantaGroupResponse>> GetSecretSantaGroups()
    {
        logger.LogInformation("Getting secret santa groups");
        var secretSantaGroups = (await uow.SecretSantaGroups.GetAll()).ToArray();
        logger.LogInformation("Found {Length} secrets santa groups", secretSantaGroups.Length);
        return secretSantaGroups.Select(group => group.ToSecretSantaGroupResponse());
    }

    public async Task<SecretSantaGroupResponse?> GetSecretSantaGroup(int secretSantaGroupId)
    {
        logger.LogInformation("Getting secret santa group {SecretSantaGroupId}", secretSantaGroupId);
        var secretSantaGroup = await uow.SecretSantaGroups.GetByIdNoTracking(secretSantaGroupId);
        logger.LogInformation("Found secret santa group {SecretSantaGroupId}", secretSantaGroupId);
        return secretSantaGroup?.ToSecretSantaGroupResponse();
    }

    public async Task<SecretSantaGroupResponse> CreateSecretSantaGroup(SecretSantaGroupRequest secretSantaGroup)
    {
        logger.LogInformation("Creating secret santa group");
        var secretSantaGroupModel = new SecretSantaGroupModel
        {
            Name = secretSantaGroup.Name
        };

        foreach (var user in secretSantaGroup.Users)
        {
            var userModel = await uow.Users.GetById(user.Id);
            if (userModel == null)
            {
                throw new InvalidOperationException($"User with id {user.Id} does not exist");
            }

            secretSantaGroupModel.SecretSantaGroupUsers.Add(new SecretSantaGroupDiscordUserModel
            {
                DiscordUser = userModel,
                SecretSantaGroup = secretSantaGroupModel
            });
        }

        await uow.SecretSantaGroups.Add(secretSantaGroupModel);
        await uow.CompleteAsync();
        
        logger.LogInformation("Finished creating secret santa group with id of {SecretSantaGroupId}", secretSantaGroup.Id);
        return secretSantaGroupModel.ToSecretSantaGroupResponse();
    }

    public async Task<SecretSantaGroupResponse> AddUserToGroup(int secretSantaGroupId, UserResponse user)
    {
        logger.LogInformation("Adding user {UserId} to secret santa group {SecretSantaGroupId}", user.Id, secretSantaGroupId);
        var secretSantaGroupModel = await uow.SecretSantaGroups.GetById(secretSantaGroupId);
        if (secretSantaGroupModel == null)
        {
            throw new ModelNotFoundException<SecretSantaGroupModel, int>(secretSantaGroupId);
        }

        var userModel = await uow.Users.GetById(user.Id);
        if (userModel == null)
        {
            throw new InvalidOperationException($"User with id {user.Id} does not exist");
        }

        var secretSantaGroupUserModel = new SecretSantaGroupDiscordUserModel
        {
            DiscordUser = userModel,
            SecretSantaGroup = secretSantaGroupModel
        };
        secretSantaGroupModel.SecretSantaGroupUsers.Add(secretSantaGroupUserModel);
        await uow.CompleteAsync();
        
        logger.LogInformation("Finished adding user {UserId} to secret santa group {SecretSantaGroupId}", user.Id, secretSantaGroupId);
        return secretSantaGroupModel.ToSecretSantaGroupResponse();
    }

    public async Task<SecretSantaGroupResponse> RemoveUserFromGroup(int secretSantaGroupId, ulong userId)
    {
        logger.LogInformation("Removing user {UserId} from secret santa group {SecretSantaGroupId}", userId, secretSantaGroupId);
        var secretSantaGroupModel = await uow.SecretSantaGroups.GetById(secretSantaGroupId);
        if (secretSantaGroupModel == null)
        {
            throw new ModelNotFoundException<SecretSantaGroupModel, int>(secretSantaGroupId);
        }

        var secretSantaGroupUserModel =
            secretSantaGroupModel.SecretSantaGroupUsers.FirstOrDefault(ssgu => ssgu.DiscordUserId == userId);
        if (secretSantaGroupUserModel == null)
        {
            throw new  InvalidOperationException($"User with id {userId} does not exist");
        }

        secretSantaGroupModel.SecretSantaGroupUsers.Remove(secretSantaGroupUserModel);
        await uow.CompleteAsync();
        
        logger.LogInformation("Finished removing user {UserId} from secret santa group {SecretSantaGroupId}", userId, secretSantaGroupId);
        return secretSantaGroupModel.ToSecretSantaGroupResponse();
    }

    public async Task<IEnumerable<SecretSantaPairResponse>> GeneratePairsForCurrentYear(int secretSantaGroupId)
    {
        logger.LogInformation("Generating pairs for secret santa group {SecretSantaGroupId}", secretSantaGroupId);
        var secretSantaGroup = await uow.SecretSantaGroups.GetById(secretSantaGroupId);
        if (secretSantaGroup == null)
        {
            throw new ModelNotFoundException<SecretSantaGroupModel, int>(secretSantaGroupId);
        }

        var currentYear = DateTime.Now.Year;
        var existingPairsInYear = (await uow.SecretSantaGroups.GetPairs(secretSantaGroupId, currentYear)).ToArray();
        if (existingPairsInYear.Length != 0)
        {
            throw new InvalidOperationException("Pairs already exists for current year");
        }

        var previousYearPairs = (await uow.SecretSantaGroups.GetPairs(secretSantaGroupId, currentYear - 1)).ToArray();
        var availableGivers = secretSantaGroup.SecretSantaGroupUsers.Select(ssgu => ssgu.DiscordUser).ToList();
        var availableRecipients = secretSantaGroup.SecretSantaGroupUsers.Select(ssgu => ssgu.DiscordUser).ToList();
        var newPairs = new List<SecretSantaPairModel>();
        while (availableGivers.Count > 0)
        {
            var giver = availableGivers[0];
            var validRecipients = availableRecipients.Where(r => IsAllowedPair(giver, r, previousYearPairs)).ToArray();
            
            if (validRecipients.Length == 0)
            {
                var swappable = newPairs.Where(p =>
                    IsAllowedPair(giver, p.RecipientDiscordUser, previousYearPairs)).ToArray();
                var swap = swappable[random.Next(swappable.Length)];
                availableGivers.Add(swap.GiverDiscordUser);
                swap.GiverDiscordUser = giver;
                swap.GiverDiscordUserId = giver.Id;
            }
            else
            {
                var recipient = validRecipients[random.Next(validRecipients.Length)];
                availableRecipients.Remove(recipient);
                newPairs.Add(new SecretSantaPairModel
                {
                    SecretSantaGroup = secretSantaGroup,
                    SecretSantaGroupId = secretSantaGroupId,
                    GiverDiscordUser = giver,
                    GiverDiscordUserId = giver.Id,
                    RecipientDiscordUser = recipient,
                    RecipientDiscordUserId = recipient.Id,
                    Year = currentYear
                });
            }
            
            availableGivers.Remove(giver);
        }

        foreach (var pair in newPairs)
        {
            secretSantaGroup.SecretSantaPairs.Add(pair);
        }
        await uow.CompleteAsync();
        
        logger.LogInformation("Finished generating pairs for secret santa group {SecretSantaGroupId}", secretSantaGroupId);
        return newPairs.Select(pair => pair.ToSecretSantaPairResponse());
    }

    public async Task SendPairs(IEnumerable<SecretSantaPairResponse> pairs)
    {
        logger.LogInformation("Sending secret santa pairs");
        foreach (var pair in pairs)
        {
            var socketUser = await client.GetUserAsync(pair.Giver.Id);
            await socketUser.SendMessageAsync($"You have {pair.Recipient.Username}! :santa:");
        }
        logger.LogInformation("Finished sending pairs for secret santa pairs");
    }

    private bool IsAllowedPair(DiscordUserModel giver, DiscordUserModel recipient, ICollection<SecretSantaPairModel> previousYearPairs)
    {
        if (recipient.Id == giver.Id)
        {
            return false;
        }

        return !previousYearPairs.Any(p => p.GiverDiscordUserId == giver.Id && p.RecipientDiscordUserId == recipient.Id);
    }
}