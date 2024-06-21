using AutoMapper;
using Brobot.Exceptions;
using Brobot.Models;
using Brobot.Profiles;
using Brobot.Repositories;
using Brobot.Services;
using Discord.WebSocket;
using Moq;

namespace Brobot.Tests;

[TestFixture]
public class SecretSantaService_GeneratePairsForCurrentYear
{

    [Test]
    public void ThrowsExceptionWhenGroupDoesNotExist()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var secretSantaRepositoryMock = new Mock<ISecretSantaGroupRepository>(MockBehavior.Strict);
        secretSantaRepositoryMock.Setup((ssr) => ssr.GetById(It.IsAny<int>()))
            .Returns(Task.FromResult<SecretSantaGroupModel?>(null));
        unitOfWorkMock.Setup((uow) => uow.SecretSantaGroups).Returns(secretSantaRepositoryMock.Object);
        var discordClientMock = new Mock<DiscordSocketClient>();
        var mapperMock = new Mock<IMapper>();

        var random = new Random();
        var secretSantaService =
            new SecretSantaService(unitOfWorkMock.Object, mapperMock.Object, discordClientMock.Object, random); 
        
        // Act/Assert
        Assert.ThrowsAsync<ModelNotFoundException<SecretSantaGroupModel, int>>(() => secretSantaService.GeneratePairsForCurrentYear(1));
    }

    [Test]
    public void ThrowsExceptionWhenGroupHasCurrentYearPairings()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        var currentYear = DateTime.Now.Year;
        var secretSantaGroupModel = SetupSecretSantaGroupModel(currentYear);        

        var secretSantaRepositoryMock = new Mock<ISecretSantaGroupRepository>(MockBehavior.Strict);
        secretSantaRepositoryMock.Setup((ssr) => ssr.GetById(1))
            .ReturnsAsync(() => secretSantaGroupModel);
        secretSantaRepositoryMock.Setup((ssr) => ssr.GetPairs(1, currentYear))
            .ReturnsAsync(() => secretSantaGroupModel.SecretSantaPairs);
        
        unitOfWorkMock.Setup((uow) => uow.SecretSantaGroups).Returns(() => secretSantaRepositoryMock.Object);
        var discordClientMock = new Mock<DiscordSocketClient>();
        var mapperMock = new Mock<IMapper>();
        var random = new Random();
        var secretSantaService =
            new SecretSantaService(unitOfWorkMock.Object, mapperMock.Object, discordClientMock.Object, random);

        // Act/Assert
        Assert.ThrowsAsync<Exception>(() => secretSantaService.GeneratePairsForCurrentYear(1));
    }

    [Test]
    public async Task GeneratesValidPairs()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        var lastYear = DateTime.Now.Year - 1;
        var secretSantaGroupModel = SetupSecretSantaGroupModel(lastYear);
        
        var secretSantaRepositoryMock = new Mock<ISecretSantaGroupRepository>(MockBehavior.Strict);
        secretSantaRepositoryMock.Setup((ssr) => ssr.GetById(1))
            .ReturnsAsync(() => secretSantaGroupModel);
        secretSantaRepositoryMock.Setup((ssr) => ssr.GetPairs(1, lastYear))
            .ReturnsAsync(() => secretSantaGroupModel.SecretSantaPairs);
        secretSantaRepositoryMock.Setup((ssr) => ssr.GetPairs(1, DateTime.Now.Year))
            .ReturnsAsync(Array.Empty<SecretSantaPairModel>);
        
        unitOfWorkMock.Setup((uow) => uow.SecretSantaGroups).Returns(() => secretSantaRepositoryMock.Object);
        unitOfWorkMock.Setup((uow) => uow.CompleteAsync())
            .ReturnsAsync(() => 0);
        var discordClientMock = new Mock<DiscordSocketClient>();
        var profile = new BrobotProfile();
        var configuration = new MapperConfiguration((config) => config.AddProfile(profile));
        var mapper = new Mapper(configuration);
        var random = new Random();
        var secretSantaService =
            new SecretSantaService(unitOfWorkMock.Object, mapper, discordClientMock.Object, random);
        
        // Act
        var pairs = await secretSantaService.GeneratePairsForCurrentYear(1);

        // Assert
        var giverIds = new HashSet<ulong>();
        var recipientIds = new HashSet<ulong>();
        foreach (var pair in pairs)
        {
            Assert.IsFalse(giverIds.Contains(pair.Giver.Id));
            Assert.IsFalse(recipientIds.Contains(pair.Recipient.Id));
            var lastYearPair = secretSantaGroupModel.SecretSantaPairs.FirstOrDefault((ssp) =>
                ssp.Year == lastYear && ssp.GiverUserId == pair.Giver.Id && ssp.RecipientUserId == pair.Recipient.Id);
            Assert.IsNull(lastYearPair);
            giverIds.Add(pair.Giver.Id);
            recipientIds.Add(pair.Recipient.Id);
        }
    }

    private SecretSantaGroupModel SetupSecretSantaGroupModel(int yearOfPairs)
    {
        var users = new List<UserModel>
        {
            new()
            {
                Id = 1,
                Username = "Evan"
            },
            new()
            {
                Id = 2,
                Username = "Peter"
            },
            new()
            {
                Id = 3,
                Username = "Ryan"
            },
            new()
            {
                Id = 4,
                Username = "JJ"
            },
            new()
            {
                Id = 5,
                Username = "Puff"
            },
            new()
            {
                Id = 6,
                Username = "Derek"
            }
        };
        
        var secretSantaGroupModel = new SecretSantaGroupModel
        {
            Id = 1,
            Name = "Test"
        };

        foreach (var user in users)
        {
            secretSantaGroupModel.SecretSantaGroupUsers.Add(new SecretSantaGroupUserModel
            {
                SecretSantaGroup = secretSantaGroupModel,
                SecretSantaGroupId = 1,
                User = user,
                UserId = user.Id
            });
        }
        
        secretSantaGroupModel.SecretSantaPairs.Add(new SecretSantaPairModel
        {
            SecretSantaGroup = secretSantaGroupModel,
            SecretSantaGroupId = secretSantaGroupModel.Id,
            GiverUser = users[0],
            GiverUserId = users[0].Id,
            RecipientUser = users[5],
            RecipientUserId = users[5].Id,
            Year = yearOfPairs
        });
        secretSantaGroupModel.SecretSantaPairs.Add(new SecretSantaPairModel
        {
            SecretSantaGroup = secretSantaGroupModel,
            SecretSantaGroupId = secretSantaGroupModel.Id,
            GiverUser = users[1],
            GiverUserId = users[1].Id,
            RecipientUser = users[0],
            RecipientUserId = users[0].Id,
            Year = yearOfPairs
        });
        
        secretSantaGroupModel.SecretSantaPairs.Add(new SecretSantaPairModel
        {
            SecretSantaGroup = secretSantaGroupModel,
            SecretSantaGroupId = secretSantaGroupModel.Id,
            GiverUser = users[3],
            GiverUserId = users[3].Id,
            RecipientUser = users[1],
            RecipientUserId = users[1].Id,
            Year = yearOfPairs
        });
        
        secretSantaGroupModel.SecretSantaPairs.Add(new SecretSantaPairModel
        {
            SecretSantaGroup = secretSantaGroupModel,
            SecretSantaGroupId = secretSantaGroupModel.Id,
            GiverUser = users[5],
            GiverUserId = users[5].Id,
            RecipientUser = users[3],
            RecipientUserId = users[3].Id,
            Year = yearOfPairs
        });
        
        secretSantaGroupModel.SecretSantaPairs.Add(new SecretSantaPairModel
        {
            SecretSantaGroup = secretSantaGroupModel,
            SecretSantaGroupId = secretSantaGroupModel.Id,
            GiverUser = users[4],
            GiverUserId = users[4].Id,
            RecipientUser = users[2],
            RecipientUserId = users[2].Id,
            Year = yearOfPairs
        });
        
        secretSantaGroupModel.SecretSantaPairs.Add(new SecretSantaPairModel
        {
            SecretSantaGroup = secretSantaGroupModel,
            SecretSantaGroupId = secretSantaGroupModel.Id,
            GiverUser = users[2],
            GiverUserId = users[2].Id,
            RecipientUser = users[4],
            RecipientUserId = users[4].Id,
            Year = yearOfPairs
        });

        return secretSantaGroupModel;
    }
}