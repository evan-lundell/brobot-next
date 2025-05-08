using TimeZoneConverter;

namespace Brobot.Tests.MessageCountServiceTests;

public class GetTotalDailyMessageCountsByChannelTests : MessageCountServiceTestBase
{
    [Test]
    public async Task UserTimezoneExists_ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);

        var counts = (await MessageCountService.GetTotalDailyMessageCountsByChannel(10, 1, "america/chicago"))
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(counts[0].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-9) + offset)));
            Assert.That(counts[0].MessageCount, Is.EqualTo(0));
            Assert.That(counts[9].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(counts[9].MessageCount, Is.EqualTo(25));
        });
    }
    
    [Test]
    public async Task WhenUserHasNoTimezone_ReturnsEmptyArray()
    {
        var counts = (await MessageCountService.GetTotalDailyMessageCountsByChannel(10, 1, null)).ToList();
        Assert.That(counts.Count, Is.EqualTo(0));
    }
}