using System.Linq.Expressions;
using AutoMapper;
using Brobot.Models;
using Brobot.Profiles;
using Brobot.Repositories;
using Brobot.Services;
using Moq;

namespace Brobot.Tests;

[TestFixture]
public class MessageCountService_GetUsersDailyMessageCounts
{
    private IMapper? _mapper;
    
    [Test]
    public async Task ReturnsEmptyArrayWhenNoTimezonePresent()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        if (_mapper == null)
        {
            throw new Exception($"{nameof(_mapper)} is null");
        }

        var service = new MessageCountService(unitOfWorkMock.Object, _mapper);
        var user = new UserModel
        {
            Id = 1,
            Username = "Test"
        };

        // Act
        var results = await service.GetUsersTotalDailyMessageCounts(user, 10);
        
        // Assert
        Assert.That(results.Count(), Is.EqualTo(0));
    }

    [Test]
    [TestCase(10, 10)]
    [TestCase(10, 5)]
    [TestCase(10, 20)]
    [TestCase(5, 10)]
    [TestCase(1, 10)]

    public async Task ReturnsCorrectNumberOfDays(int numOfDays, int countsInDatabase)
    {
        // Arrange
        ulong userId = 1;
        var user = new UserModel
        {
            Id = userId,
            Username = "Test",
            Timezone = "America/Chicago"
        };
        
        var channel = new ChannelModel
        {
            Id = 1,
            Name = "TestChannel",
            Guild = new GuildModel
            {
                Id = 1,
                Name = "TestGuild"
            },
            GuildId =  1
        };
        
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var dailyMessageCountsRepoMock = new Mock<IDailyMessageCountRepository>(MockBehavior.Strict);

        var messageCountModels = new DailyMessageCountModel[countsInDatabase];
        var random = new Random();
        var today = DateOnly.FromDateTime(DateTime.Now);
        for (var i = 0; i < messageCountModels.Length; i++)
        {
            messageCountModels[i] = new DailyMessageCountModel
            {
                UserId = userId,
                User = user,
                Channel = channel,
                ChannelId = channel.Id,
                CountDate = today.AddDays(i * -1),
                MessageCount = random.Next(1, 100)
            };
        }

        dailyMessageCountsRepoMock.Setup((m) =>
                m.Find(It.IsAny<Expression<Func<DailyMessageCountModel, bool>>>()))
            .ReturnsAsync(messageCountModels);
        unitOfWorkMock.Setup((uow) => uow.DailyMessageCounts).Returns(dailyMessageCountsRepoMock.Object);
        if (_mapper == null)
        {
            throw new Exception($"{nameof(_mapper)} is null");
        }
        var service = new MessageCountService(unitOfWorkMock.Object, _mapper);
        
        // Act
        var results = await service.GetUsersTotalDailyMessageCounts(user, numOfDays);
        
        // Assert
        Assert.That(results.Count(), Is.EqualTo(numOfDays));
    }
    
    [SetUp]
    public void Setup()
    {
        if (_mapper == null)
        {
            var profile = new BrobotProfile();
            var configuration = new MapperConfiguration((config) => config.AddProfile(profile));
            _mapper = new Mapper(configuration);
        }
    }
}