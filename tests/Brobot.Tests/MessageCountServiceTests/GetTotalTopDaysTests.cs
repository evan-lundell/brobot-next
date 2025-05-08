using TimeZoneConverter;

namespace Brobot.Tests.MessageCountServiceTests;

public class GetTotalTopDaysTests : MessageCountServiceTestBase
{
    [Test]
    public async Task ReturnsCorrectCounts()
    {
        var timezone = TZConvert.GetTimeZoneInfo("america/chicago");
        var offset = timezone.GetUtcOffset(DateTime.UtcNow);

        var counts = (await MessageCountService.GetTotalTopDays(10)).ToList();
        
        Assert.Multiple(() =>
        {
            Assert.That(counts[0].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2) + offset)));
            Assert.That(counts[0].MessageCount, Is.EqualTo(150));
            Assert.That(counts[1].CountDate, Is.EqualTo(DateOnly.FromDateTime(DateTime.UtcNow + offset)));
            Assert.That(counts[1].MessageCount, Is.EqualTo(35));
        });
    }
}