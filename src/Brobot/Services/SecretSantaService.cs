using AutoMapper;
using Brobot.Exceptions;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Discord;
using Discord.WebSocket;

namespace Brobot.Services;

public class SecretSantaService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly DiscordSocketClient _client;
    private readonly Random _random;

    public SecretSantaService(IUnitOfWork uow, IMapper mapper, DiscordSocketClient client, Random random)
    {
        _uow = uow;
        _mapper = mapper;
        _client = client;
        _random = random;
    }

    public async Task<IEnumerable<SecretSantaGroupResponse>> GetSecretSantaGroups()
    {
        var secretSantaGroups = (await _uow.SecretSantaGroups.GetAll()).ToArray();
        return _mapper.Map<IEnumerable<SecretSantaGroupResponse>>(secretSantaGroups);
    }

    public async Task<SecretSantaGroupResponse?> GetSecretSantaGroup(int secretSantaGroupId)
    {
        var secretSantaGroup = await _uow.SecretSantaGroups.GetByIdNoTracking(secretSantaGroupId);
        return secretSantaGroup == null ? null : _mapper.Map<SecretSantaGroupResponse>(secretSantaGroup);
    }

    public async Task<SecretSantaGroupResponse> CreateSecretSantaGroup(SecretSantaGroupRequest secretSantaGroup)
    {
        var secretSantaGroupModel = new SecretSantaGroupModel
        {
            Name = secretSantaGroup.Name
        };

        foreach (var user in secretSantaGroup.Users)
        {
            var userModel = await _uow.Users.GetById(user.Id);
            if (userModel == null)
            {
                throw new ModelNotFoundException<SecretSantaGroupModel, ulong>(user.Id);
            }

            secretSantaGroupModel.SecretSantaGroupUsers.Add(new SecretSantaGroupUserModel
            {
                User = userModel,
                SecretSantaGroup = secretSantaGroupModel
            });
        }

        await _uow.SecretSantaGroups.Add(secretSantaGroupModel);
        await _uow.CompleteAsync();
        return _mapper.Map<SecretSantaGroupResponse>(secretSantaGroupModel);
    }

    public async Task<SecretSantaGroupResponse> AddUserToGroup(int secretSantaGroupId, UserResponse user)
    {
        var secretSantaGroupModel = await _uow.SecretSantaGroups.GetById(secretSantaGroupId);
        if (secretSantaGroupModel == null)
        {
            throw new ModelNotFoundException<SecretSantaGroupModel, int>(secretSantaGroupId);
        }

        var userModel = await _uow.Users.GetById(user.Id);
        if (userModel == null)
        {
            throw new ModelNotFoundException<UserModel, ulong>(user.Id);
        }

        var secretSantaGroupUserModel = new SecretSantaGroupUserModel
        {
            User = userModel,
            SecretSantaGroup = secretSantaGroupModel
        };
        secretSantaGroupModel.SecretSantaGroupUsers.Add(secretSantaGroupUserModel);

        await _uow.CompleteAsync();
        return _mapper.Map<SecretSantaGroupResponse>(secretSantaGroupModel);
    }

    public async Task<SecretSantaGroupResponse> RemoveUserFromGroup(int secretSantaGroupId, ulong userId)
    {
        var secretSantaGroupModel = await _uow.SecretSantaGroups.GetById(secretSantaGroupId);
        if (secretSantaGroupModel == null)
        {
            throw new ModelNotFoundException<SecretSantaGroupModel, int>(secretSantaGroupId);
        }

        var secretSantaGroupUserModel =
            secretSantaGroupModel.SecretSantaGroupUsers.FirstOrDefault((ssgu) => ssgu.UserId == userId);
        if (secretSantaGroupUserModel == null)
        {
            return _mapper.Map<SecretSantaGroupResponse>(secretSantaGroupModel);
        }

        secretSantaGroupModel.SecretSantaGroupUsers.Remove(secretSantaGroupUserModel);
        await _uow.CompleteAsync();
        return _mapper.Map<SecretSantaGroupResponse>(secretSantaGroupModel);
    }

    public async Task<IEnumerable<SecretSantaPairResponse>> GeneratePairsForCurrentYear(int secretSantaGroupId)
    {
        var secretSantaGroup = await _uow.SecretSantaGroups.GetById(secretSantaGroupId);
        if (secretSantaGroup == null)
        {
            throw new ModelNotFoundException<SecretSantaGroupModel, int>(secretSantaGroupId);
        }

        var currentYear = DateTime.Now.Year;
        var existingPairsInYear = (await _uow.SecretSantaGroups.GetPairs(secretSantaGroupId, currentYear)).ToArray();
        if (existingPairsInYear.Length != 0)
        {
            throw new Exception("Pairs already exists for current year");
        }

        var previousYearPairs = (await _uow.SecretSantaGroups.GetPairs(secretSantaGroupId, currentYear - 1)).ToArray();
        var availableGivers = secretSantaGroup.SecretSantaGroupUsers.Select(ssgu => ssgu.User).ToList();
        var availableRecipients = secretSantaGroup.SecretSantaGroupUsers.Select(ssgu => ssgu.User).ToList();
        var newPairs = new List<SecretSantaPairModel>();
        while (availableGivers.Count > 0)
        {
            var giver = availableGivers[0];
            var validRecipients = availableRecipients.Where((r) => IsAllowedPair(giver, r, previousYearPairs)).ToArray();
            
            if (validRecipients.Length == 0)
            {
                var swappable = newPairs.Where(p =>
                    IsAllowedPair(giver, p.RecipientUser, previousYearPairs)).ToArray();
                var swap = swappable[_random.Next(validRecipients.Length)];
                availableGivers.Add(swap.GiverUser);
                swap.GiverUser = giver;
                swap.GiverUserId = giver.Id;
            }
            else
            {
                var recipient = validRecipients[_random.Next(validRecipients.Length)];
                availableRecipients.Remove(recipient);
                newPairs.Add(new SecretSantaPairModel
                {
                    SecretSantaGroup = secretSantaGroup,
                    SecretSantaGroupId = secretSantaGroupId,
                    GiverUser = giver,
                    GiverUserId = giver.Id,
                    RecipientUser = recipient,
                    RecipientUserId = recipient.Id,
                    Year = currentYear
                });
            }
            
            availableGivers.Remove(giver);
        }

        foreach (var pair in newPairs)
        {
            secretSantaGroup.SecretSantaPairs.Add(pair);
        }
        await _uow.CompleteAsync();
        return _mapper.Map<IEnumerable<SecretSantaPairResponse>>(newPairs);
    }

    public async Task SendPairs(IEnumerable<SecretSantaPairResponse> pairs)
    {
        foreach (var pair in pairs)
        {
            var socketUser = await _client.GetUserAsync(pair.Giver.Id);
            await socketUser.SendMessageAsync($"You have {pair.Recipient.Username}! :santa:");
        }
    }

    private bool IsAllowedPair(UserModel giver, UserModel recipient, ICollection<SecretSantaPairModel> previousYearPairs)
    {
        if (recipient.Id == giver.Id)
        {
            return false;
        }

        return !previousYearPairs.Any((p) => p.GiverUserId == giver.Id && p.RecipientUserId == recipient.Id);
    }
}