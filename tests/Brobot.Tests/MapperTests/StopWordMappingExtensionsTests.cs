using Brobot.Models;
using Brobot.Mappers;
using Brobot.Shared.Requests;

namespace Brobot.Tests.MapperTests;

[TestFixture]
public class StopWordMappingExtensionsTests
{
    [Test]
    public void ToStopWordResponse_MapsModelToResponse()
    {
        var model = new StopWordModel { Id = 5, Word = "foo" };
        var response = model.ToStopWordResponse();
        Assert.That(response.Id, Is.EqualTo(5));
        Assert.That(response.Word, Is.EqualTo("foo"));
    }

    [Test]
    public void ToStopWordModel_MapsRequestToModel()
    {
        var request = new StopWordRequest { Word = "bar" };
        var model = request.ToStopWordModel();
        Assert.That(model.Word, Is.EqualTo("bar"));
    }
}
