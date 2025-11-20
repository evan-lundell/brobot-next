using Brobot.Repositories;
using Brobot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TimeZoneConverter;

namespace Brobot.Tests.MessageCountServiceTests;

public class AddToDailyCountTests : MessageCountServiceTestBase
{
    [Test]
    public async Task DailyMessageCountExists_AddsToExistingCount()
    {
        ulong userId = 1;
        ulong channelId = 1;
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var countDate = DateOnly.FromDateTime(DateTime.UtcNow + timezone.GetUtcOffset(DateTime.UtcNow));

        await MessageCountService.AddToDailyCount(userId, channelId, countDate);

        var counts = await Context.DailyMessageCounts
            .Where(dmc => dmc.UserId == userId && dmc.ChannelId == channelId && dmc.CountDate == countDate)
            .ToListAsync();
        Assert.That(counts.Count, Is.EqualTo(1));
        Assert.That(counts[0].MessageCount, Is.EqualTo(11));

    }

    [Test]
    public async Task NoDailyMessageCount_AddsNewCount()
    {
        ulong userId = 1;
        ulong channelId = 1;
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var countDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3) + timezone.GetUtcOffset(DateTime.UtcNow));

        await MessageCountService.AddToDailyCount(userId, channelId, countDate);

        var counts = await Context.DailyMessageCounts
            .Where(dmc => dmc.UserId == userId && dmc.ChannelId == channelId && dmc.CountDate == countDate)
            .ToListAsync();
        Assert.That(counts.Count, Is.EqualTo(1));
        Assert.That(counts[0].MessageCount, Is.EqualTo(1));
    }
    
    [Test]
    public async Task UserHasNoTimezone_DoesNotAddCount()
    {
        ulong userId = 4;
        ulong channelId = 1;

        await MessageCountService.AddToDailyCount(userId, channelId);

        var counts = await Context.DailyMessageCounts
            .Where(dmc => dmc.UserId == userId && dmc.ChannelId == channelId && dmc.CountDate == DateOnly.FromDateTime(DateTime.Now))
            .ToListAsync();
        Assert.That(counts.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task NoCountDate_AddsToToday()
    {
        var userId = 5UL;
        var channelId = 1UL;
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var today = DateOnly.FromDateTime(DateTime.UtcNow + timezone.GetUtcOffset(DateTime.UtcNow));
        
        await MessageCountService.AddToDailyCount(userId, channelId);
        
        var counts = await Context.DailyMessageCounts
            .Where(dmc => dmc.UserId == userId && dmc.ChannelId == channelId && dmc.CountDate == today)
            .ToListAsync();
        Assert.That(counts.Count, Is.EqualTo(1));
        
    }

    [Test]
    public async Task InvalidChannelId_NothingAdded()
    {
        var userId = 5UL;
        var channelId = 100UL;

        var count = await Context.DailyMessageCounts.CountAsync();
        var sum = await Context.DailyMessageCounts.SumAsync(dmc => dmc.MessageCount);
        
        await MessageCountService.AddToDailyCount(userId, channelId);
        
        var newCount = await Context.DailyMessageCounts.CountAsync();
        var newSum = await Context.DailyMessageCounts.SumAsync(dmc => dmc.MessageCount);
        Assert.Multiple(() =>
        {
            Assert.That(newCount, Is.EqualTo(count));
            Assert.That(newSum, Is.EqualTo(sum));
        });
    }
    
    [Test]
    public async Task WhenExceptionOccurs_LogsError()
    {
        // Arrange
        var userId = 123ul;
        var channelId = 456ul;
        var mockUow = new Mock<IUnitOfWork>();
        var mockLogger = new Mock<ILogger<MessageCountService>>();
    
        mockUow.Setup(x => x.Users.GetById(userId))
            .ThrowsAsync(new Exception("Database error"));
    
        var service = new MessageCountService(mockUow.Object, mockLogger.Object);
    
        // Act
        await service.AddToDailyCount(userId, channelId);
    
        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error adding daily count for user {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}