using Brobot.Dtos;
using Brobot.Mappers;

namespace Brobot.Tests.MapperTests;

[TestFixture]
public class ScoreboardMappingExtensionsTests
{
    [Test]
    public void ToScoreboardResponse_MapsDtoToResponse()
    {
        var scores = new List<ScoreboardItemDto>
        {
            new ScoreboardItemDto { UserId = 1, Username = "a", Score = 10 },
            new ScoreboardItemDto { UserId = 2, Username = "b", Score = 20 }
        };
        var dto = new ScoreboardDto { HotOpId = 5, OwnerUsername = "owner", Scores = scores };
        var response = dto.ToScoreboardResponse();
        var responseScores = response.Scores.ToList();
        Assert.That(response.HotOpId, Is.EqualTo(5));
        Assert.That(response.OwnerUsername, Is.EqualTo("owner"));
        Assert.That(response.Scores.Count, Is.EqualTo(2));
        Assert.That(responseScores[0].UserId, Is.EqualTo(1));
        Assert.That(responseScores[1].Score, Is.EqualTo(20));
    }

    [Test]
    public void ToScoreboardItemResponse_MapsDtoToResponse()
    {
        var dto = new ScoreboardItemDto { UserId = 3, Username = "c", Score = 30 };
        var response = dto.ToScoreboardItemResponse();
        Assert.That(response.UserId, Is.EqualTo(3));
        Assert.That(response.Username, Is.EqualTo("c"));
        Assert.That(response.Score, Is.EqualTo(30));
    }
}
