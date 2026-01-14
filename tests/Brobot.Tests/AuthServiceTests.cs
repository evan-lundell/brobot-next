using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Brobot.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;

namespace Brobot.Tests;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<UserManager<ApplicationUserModel>> _userManagerMock;
    private Mock<ILogger<AuthService>> _loggerMock;
    private AuthService _authService;
    
    [SetUp]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var userStoreMock = new Mock<IUserStore<ApplicationUserModel>>();
        _userManagerMock = new Mock<UserManager<ApplicationUserModel>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _loggerMock = new Mock<ILogger<AuthService>>();
        
        _authService = new AuthService(
            _userManagerMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task GetOrCreateApplicationUserAsync_UserDoesNotExistInDatabase_ReturnsFailure()
    {
        // Arrange
        var discordUserId = 123456789UL;
        _unitOfWorkMock.Setup(uow => uow.Users.GetById(discordUserId))
            .ReturnsAsync((DiscordUserModel?)null);

        // Act
        var result = await _authService.GetOrCreateApplicationUserAsync(discordUserId);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("You are not authorized to access this application. Please contact an administrator."));
        }
    }

    [Test]
    public async Task GetOrCreateApplicationUserAsync_ExistingApplicationUserWithRole_ReturnsSuccess()
    {
        // Arrange
        const ulong discordUserId = 123456789UL;
        const string discordUsername = "TestUser";
        var discordUser = new DiscordUserModel { Id = discordUserId, Username = discordUsername };
        var existingAppUser = new ApplicationUserModel
        {
            UserName = discordUsername,
            DiscordUser = discordUser,
            DiscordUserId = discordUserId
        };
        
        _unitOfWorkMock.Setup(uow => uow.Users.GetById(discordUserId))
            .ReturnsAsync(discordUser);
        
        var users = new List<ApplicationUserModel> { existingAppUser }
            .AsQueryable()
            .AsEnumerable()
            .BuildMock();
        
        _userManagerMock.Setup(um => um.Users)
            .Returns(users);
        _userManagerMock.Setup(um => um.GetRolesAsync(existingAppUser))
            .ReturnsAsync(new List<string> { "User" });
        
        // Act
        var result = await _authService.GetOrCreateApplicationUserAsync(discordUserId);
        
        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
            Assert.That(result.User, Is.SameAs(existingAppUser));
            Assert.That(result.DiscordUser, Is.SameAs(discordUser));
            Assert.That(result.Roles, Is.EquivalentTo(new List<string> { "User" }));
        }
    }

    [Test]
    public async Task GetOrCreateApplicationUserAsync_NewApplicationUserCreation_ReturnsSuccess()
    {
        // Arrange
        const ulong discordUserId = 123456789UL;
        const string discordUsername = "NewUser";
        var discordUser = new DiscordUserModel { Id = discordUserId, Username = discordUsername };
        _unitOfWorkMock.Setup(uow => uow.Users.GetById(discordUserId))
            .ReturnsAsync(discordUser);
        var users = new List<ApplicationUserModel>()
            .AsQueryable()
            .AsEnumerable()
            .BuildMock();
        _userManagerMock.Setup(um => um.Users)
            .Returns(users);
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUserModel>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUserModel>(), Constants.UserRoleName))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUserModel>()))
            .ReturnsAsync(new List<string> { Constants.UserRoleName });
        
        // Act
        var result = await _authService.GetOrCreateApplicationUserAsync(discordUserId);
        
        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
            Assert.That(result.User, Is.Not.Null);
            Assert.That(result.User.DiscordUserId, Is.EqualTo(discordUserId));
            Assert.That(result.DiscordUser, Is.SameAs(discordUser));
            Assert.That(result.Roles, Is.EquivalentTo(new List<string> { Constants.UserRoleName }));
        }
    }

    [Test]
    public async Task GetOrCreateApplicationUserAsync_NewApplicationUserCreationFails_ReturnsFailure()
    {
        // Arrange
        const ulong discordUserId = 123456789UL;
        const string discordUsername = "NewUser";
        var discordUser = new DiscordUserModel { Id = discordUserId, Username = discordUsername };
        _unitOfWorkMock.Setup(uow => uow.Users.GetById(discordUserId))
            .ReturnsAsync(discordUser);
        var users = new List<ApplicationUserModel>()
            .AsQueryable()
            .AsEnumerable()
            .BuildMock();
        _userManagerMock.Setup(um => um.Users)
            .Returns(users);
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUserModel>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed" }));

        // Act
        var result = await _authService.GetOrCreateApplicationUserAsync(discordUserId);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Failed to create user account. Please try again."));
        }
    }

    [Test]
    public async Task GetOrCreateApplicationUserAsync_RoleAssignmentFails_ReturnsFailure()
    {
        // Arrange
        const ulong discordUserId = 123456789UL;
        const string discordUsername = "NewUser";
        var discordUser = new DiscordUserModel { Id = discordUserId, Username = discordUsername };
        _unitOfWorkMock.Setup(uow => uow.Users.GetById(discordUserId))
            .ReturnsAsync(discordUser);
        var users = new List<ApplicationUserModel>()
            .AsQueryable()
            .AsEnumerable()
            .BuildMock();
        _userManagerMock.Setup(um => um.Users)
            .Returns(users);
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUserModel>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUserModel>(), Constants.UserRoleName))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed" }));
        
        // Act
        var result = await _authService.GetOrCreateApplicationUserAsync(discordUserId);
        
        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Failed to assign user role. Please contact an administrator."));
        }
    }

    [Test]
    public async Task GetOrCreateApplicationUserAsync_ExistingApplicationUserWithoutRole_ReturnsFailure()
    {
        // Arrange
        const ulong discordUserId = 123456789UL;
        const string discordUsername = "NewUser";
        var discordUser = new DiscordUserModel { Id = discordUserId, Username = discordUsername };
        _unitOfWorkMock.Setup(uow => uow.Users.GetById(discordUserId))
            .ReturnsAsync(discordUser);
        var existingAppUser = new ApplicationUserModel
        {
            UserName = discordUsername,
            DiscordUser = discordUser,
            DiscordUserId = discordUserId
        };
        var users = new List<ApplicationUserModel> { existingAppUser }
            .AsQueryable()
            .AsEnumerable()
            .BuildMock();
        _userManagerMock.Setup(um => um.Users)
            .Returns(users);
        _userManagerMock.Setup(um => um.GetRolesAsync(existingAppUser))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _authService.GetOrCreateApplicationUserAsync(discordUserId);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.ErrorMessage,
                Is.EqualTo("You do not have any roles assigned. Please contact an administrator."));
        }
    }
}
