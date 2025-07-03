using Brobot.Models;
using Brobot.Mappers;

namespace Brobot.Tests.MapperTests;

[TestFixture]
public class HotOpMappingExtensionsTests
{
    [Test]
    public void ToHotOpResponse_MapsModelToResponse()
    {
        var user = new UserModel { Id = 1, Username = "user" };
        var guild = new GuildModel { Id = 1, Name = "guild" };
        var channel = new ChannelModel { Id = 2, Name = "general", Guild = guild };
        var model = new HotOpModel
        {
            Id = 42,
            User = user,
            Channel = channel,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(1)
        };

        var response = model.ToHotOpResponse();

        Assert.That(response.Id, Is.EqualTo(42));
        Assert.That(response.User.Id, Is.EqualTo(1));
        Assert.That(response.Channel.Id, Is.EqualTo(2));
        Assert.That(response.StartDate, Is.EqualTo(model.StartDate));
        Assert.That(response.EndDate, Is.EqualTo(model.EndDate));
    }
}
