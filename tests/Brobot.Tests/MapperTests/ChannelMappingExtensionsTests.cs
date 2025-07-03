using Brobot.Mappers;
using Brobot.Models;

namespace Brobot.Tests.MapperTests;

[TestFixture]
public class ChannelMappingExtensionsTests
{
    [Test]
    public void ToChannelResponse_MapsModelToResponse()
    {
        var guildModel = new GuildModel { Id = 1, Name = "guild" };
        var model = new ChannelModel { Guild = guildModel, Id = 42, Name = "general" };

        var response = model.ToChannelResponse();

        Assert.That(response.Id, Is.EqualTo(42));
        Assert.That(response.Name, Is.EqualTo("general"));
    }
}