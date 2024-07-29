using TimeZoneConverter;

namespace Brobot.Tests.MessageCountServiceTests;

public class GetTotalTopDaysByChannelTests : MessageCountServiceTestBase
{
    [Test]
    public async Task ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);

        var counts = (await MessageCountService.GetTotalTopDaysByChannel(1UL, 10)).ToList();
        
        Assert.Multiple(() =>
        {
            Assert.That(counts[0].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2) + offset)));
            Assert.That(counts[0].MessageCount, Is.EqualTo(90));
            Assert.That(counts[1].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(counts[1].MessageCount, Is.EqualTo(25));
        });
    }
}