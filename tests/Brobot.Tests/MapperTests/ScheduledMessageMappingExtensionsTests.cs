using Brobot.Models;
using Brobot.Mappers;

namespace Brobot.Tests.MapperTests;

[TestFixture]
public class ScheduledMessageMappingExtensionsTests
{
    [Test]
    public void ToScheduledMessageResponse_MapsModelToResponse()
    {
        var guild = new GuildModel { Id = 1, Name = "guild" };
        var channel = new ChannelModel { Id = 1, Name = "general", Guild = guild };
        var user  = new UserModel { Id = 99, Username = "user" };
        var model = new ScheduledMessageModel
        {
            Id = 10,
            MessageText = "Hello!",
            SendDate = DateTime.UtcNow,
            SentDate = DateTime.UtcNow.AddMinutes(1),
            Channel = channel,
            CreatedById = 99,
            CreatedBy = user
        };
        var response = model.ToScheduledMessageResponse();
        Assert.That(response.Id, Is.EqualTo(10));
        Assert.That(response.MessageText, Is.EqualTo("Hello!"));
        Assert.That(response.SendDate, Is.EqualTo(model.SendDate));
        Assert.That(response.SentDate, Is.EqualTo(model.SentDate));
        Assert.That(response.Channel.Id, Is.EqualTo(1));
        Assert.That(response.CreatedById, Is.EqualTo(99));
    }
}
