using TimeZoneConverter;

namespace Brobot.Tests.MessageCountServiceTests;

public class GetUserDailyMessageCountForChannelTests : MessageCountServiceTestBase
{
    [Test]
    public async Task WhenUserTimezoneExists_ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);

        var counts = (await MessageCountService.GetUsersDailyMessageCountForChannel(1, 1, 10)).ToList();
        var orderedCounts = counts.OrderBy(x => x.CountDate).ToList();
        var sum = counts.Sum(c => c.MessageCount);
        Assert.Multiple(() =>
        {
            Assert.That(counts.Count, Is.EqualTo(10));
            Assert.That(orderedCounts[9].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(orderedCounts[9].MessageCount, Is.EqualTo(10));
            Assert.That(orderedCounts[0].CountDate,
                Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-9) + offset)));
            Assert.That(orderedCounts[0].MessageCount, Is.EqualTo(0));
            Assert.That(sum, Is.EqualTo(65));
        });
    }
    
    [Test]
    public async Task WhenUserHasNoTimezone_ReturnsEmptyArray()
    {
        var counts = (await MessageCountService.GetUsersDailyMessageCountForChannel(4, 1, 10)).ToList();
        Assert.That(counts.Count, Is.EqualTo(0));
    }
}