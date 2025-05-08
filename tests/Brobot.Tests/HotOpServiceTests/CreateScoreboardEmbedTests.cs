using Discord;

namespace Brobot.Tests.HotOpServiceTests;

[TestFixture]
public class CreateScoreboardEmbedTests : HotOpServiceTestsBase
{
    [Test]
    public async Task CreatesEmbed()
    {
        var hotOp = await UnitOfWork.HotOps.GetById(1);
        if (hotOp == null)
        {
            throw new Exception("Hot Op not found");
        }
        var embed = HotOpService.CreateScoreboardEmbed(hotOp);
        Assert.Multiple(() =>
        {
            Assert.That(embed.Color, Is.EqualTo(new Color(114, 137, 218)));
            Assert.That(embed.Description, Is.EqualTo($"Operation Hot Test User 1"));
            Assert.That(embed.Fields.Length, Is.EqualTo(2));
            Assert.That(embed.Fields.First().Value, Is.EqualTo("1000"));
            Assert.That(embed.Fields.Last().Value, Is.EqualTo("400"));
        });
    }
}