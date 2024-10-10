using TimeZoneConverter;

namespace Brobot.Tests.MessageCountServiceTests;

public class GetTotalDailyMessageCountsTests : MessageCountServiceTestBase
{
    [Test]
    public async Task UserTimezoneExists_ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);

        var counts = (await MessageCountService.GetTotalDailyMessageCounts(10, "america/chicago")).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(counts[0].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-9) + offset)));
            Assert.That(counts[0].MessageCount, Is.EqualTo(0));
            Assert.That(counts[9].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(counts[9].MessageCount, Is.EqualTo(35));

        });
    }
    
    [Test]
    public async Task WhenUserHasNoTimezone_ReturnsEmptyArray()
    {
        var counts = (await MessageCountService.GetTotalDailyMessageCounts(10, null)).ToList();
        Assert.That(counts.Count, Is.EqualTo(0));
    }
}