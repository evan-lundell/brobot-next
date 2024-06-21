using Brobot.Models;
using Brobot.Services;
using Moq;

namespace Brobot.Tests;

[TestFixture]
public class HotOpService_GetScoreboard
{
    [Test]
    public void ReturnsCorrectNumberOfScores()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
        var hotOpService = new HotOpService(serviceProviderMock.Object);
        var hotOpModel = SetupBasicHotOp();
        
        var numOfUser = 10;
        for (var i = 0; i < numOfUser; i++)
        {
            SetupUser(hotOpModel, (ulong)i * 1000, $"User {i}");
        }
        
        // Act
        var scoreboard = hotOpService.GetScoreboard(hotOpModel);

        // Assert
        Assert.That(numOfUser, Is.EqualTo(scoreboard.Scores.Count()));
    }

    [Test]
    public void ReturnsCorrectScore()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
        var hotOpService = new HotOpService(serviceProviderMock.Object);
        var hotOpModel = SetupBasicHotOp();
        
        var userWithScore = SetupUser(hotOpModel, 100, "HasScore");
        var userWithoutScore = SetupUser(hotOpModel, 200, "NoScore");

        var sessionLength = 10;
        SetupSession(hotOpModel, userWithScore, 1000, sessionLength);

        // Act
        var scoreboard = hotOpService.GetScoreboard(hotOpModel);

        // Assert
        var userWithScoreEntry = scoreboard.Scores.FirstOrDefault((s) => s.UserId == userWithScore.Id);
        var userWithoutScoreEntry = scoreboard.Scores.FirstOrDefault((s) => s.UserId == userWithoutScore.Id);
        Assert.That(userWithScoreEntry?.Score, Is.EqualTo(sessionLength * 10));
        Assert.That(userWithoutScoreEntry?.Score, Is.EqualTo(0));
    }

    [Test]
    public void ReturnsCorrectOrder()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
        var hotOpService = new HotOpService(serviceProviderMock.Object);
        var hotOpModel = SetupBasicHotOp();

        var thirdPlaceUser = SetupUser(hotOpModel, 100, "ThirdPlace");
        var firstPlaceUser = SetupUser(hotOpModel, 200, "FirstPlace");
        var secondPlaceUser = SetupUser(hotOpModel, 300, "SecondPlace");
        
        SetupSession(hotOpModel, thirdPlaceUser, 1000,5);
        SetupSession(hotOpModel, firstPlaceUser, 2000,20);
        SetupSession(hotOpModel, secondPlaceUser, 3000,12);
        
        // Act
        var scoreboard = hotOpService.GetScoreboard(hotOpModel);
        
        // Assert
        Assert.That(scoreboard.Scores.Count(), Is.EqualTo(3));
        var index = 0;
        var expectedValues = new[]
        {
            "FirstPlace",
            "SecondPlace",
            "ThirdPlace"
        };
        foreach (var scoreboardEntry in scoreboard.Scores)
        {
            Assert.That(scoreboardEntry.Username, Is.EqualTo(expectedValues[index]));
            index++;
        }
    }

    private HotOpModel SetupBasicHotOp()
    {
        var hotOpOwner = new UserModel
        {
            Id = 1,
            Username = "Owner"
        };
        var hotOpGuild = new GuildModel
        {
            Id = 2,
            Name = "TestGuild"
        };
        var hotOpChannel = new ChannelModel
        {
            Id = 3,
            Name = "TestChannel",
            Guild = hotOpGuild,
            GuildId = hotOpGuild.Id
        };
        
        var hotOpModel = new HotOpModel
        {
            Id = 4,
            User = hotOpOwner,
            UserId = hotOpOwner.Id,
            Channel = hotOpChannel,
            ChannelId = hotOpChannel.Id
        };
        
        hotOpChannel.ChannelUsers.Add(new ChannelUserModel
        {
            User = hotOpOwner,
            UserId = hotOpOwner.Id,
            Channel = hotOpChannel,
            ChannelId = hotOpChannel.Id
        });

        return hotOpModel;
    }

    private UserModel SetupUser(HotOpModel hotOpModel, ulong userId, string username)
    {
        var user = new UserModel
        {
            Id = userId,
            Username = username
        };
        hotOpModel.Channel.ChannelUsers.Add(new ChannelUserModel
        {
            User = user,
            UserId = user.Id,
            Channel = hotOpModel.Channel,
            ChannelId = hotOpModel.ChannelId
        });

        return user;
    }

    private void SetupSession(HotOpModel hotOpModel, UserModel userModel, int id, int length)
    {
        var endDate = DateTimeOffset.UtcNow;
        hotOpModel.HotOpSessions.Add(new HotOpSessionModel
        {
            Id = id,
            HotOp = hotOpModel,
            HotOpId = hotOpModel.Id,
            StartDateTime = endDate.AddMinutes(length * -1),
            EndDateTime = endDate,
            User = userModel,
            UserId = userModel.Id,
        });
    }
}