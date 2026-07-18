using System.Globalization;
using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class GetBirthdayTests : BrobotModuleTestBase
{
    [Test]
    public async Task UserNotFound_RespondsWithUserNotFoundMessage()
    {
        const ulong userId = 1;
        const string username = "TestUser";
        Mock<IUserRepository> userRepositoryMock = new();
        UnitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
        Mock<IUser> userMock = new();
        userMock.SetupGet(x => x.Id).Returns(userId);
        userMock.SetupGet(x => x.Username).Returns(username);

        _ = InteractionContextMock.SetupGet(x => x.User)
            .Returns(userMock.Object);
        
        await BrobotModule.GetBirthday(userMock.Object);
        
        AssertRespondAsyncCalledOnce($"{username} has not set their birthday", true);
    }

    [Test]
    public async Task UserHasNoBirthday_RespondsWithNoBirthdayMessage()
    {
        const ulong userId = 1;
        const string username = "TestUser";
        Mock<IUserRepository> userRepositoryMock = new();
        DiscordUserModel testUser = new()
        {
            Id = userId,
            Username = username,
            Birthdate = null
        };
        userRepositoryMock.Setup(u => u.GetById(userId))
            .ReturnsAsync(testUser);
        UnitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
        Mock<IUser> userMock = new();
        userMock.SetupGet(x => x.Id).Returns(userId);
        userMock.SetupGet(x => x.Username).Returns(username);
        InteractionContextMock.SetupGet(x => x.User).Returns(userMock.Object);
        
        await BrobotModule.GetBirthday(userMock.Object);
        
        AssertRespondAsyncCalledOnce($"{username} has not set their birthday", true);
    }
    
    [Test]
    public async Task GetBirthdaySucceeds_RespondsWithBirthdayMessage()
    {
        const ulong userId = 1;
        DateOnly birthdate = new(1990, 1, 1);
        Mock<IUserRepository> userRepositoryMock = new();
        DiscordUserModel testUser = new()
        {
            Id = userId,
            Username = "TestUser",
            Birthdate = birthdate
        };
        userRepositoryMock.Setup(u => u.GetById(userId))
            .ReturnsAsync(testUser);
        UnitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
        Mock<IUser> userMock = new();
        userMock.SetupGet(x => x.Id).Returns(userId);
        InteractionContextMock.SetupGet(x => x.User).Returns(userMock.Object);
        
        await BrobotModule.GetBirthday(userMock.Object);
        
        AssertRespondAsyncCalledOnce(birthdate.ToString("m", CultureInfo.InvariantCulture), true);
    }

    [Test]
    public async Task ExceptionThrown_RespondsWithErrorMessage()
    {
        const ulong userId = 1;
        Mock<IUserRepository> userRepositoryMock = new();
        userRepositoryMock.Setup(u => u.GetById(userId))
            .ThrowsAsync(new Exception("Database error"));
        UnitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
        Mock<IUser> userMock = new();
        userMock.SetupGet(x => x.Id).Returns(userId);
        InteractionContextMock.SetupGet(x => x.User).Returns(userMock.Object);
        
        await BrobotModule.GetBirthday(userMock.Object);
        
        AssertRespondAsyncCalledOnce("Failed to get birthday", true);
    }
}
