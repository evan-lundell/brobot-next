using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class PikaTests : BrobotModuleTestBase
{
    [Test]
    public async Task PikaCalled_RespondsWithPikaFile()
    {
        await BrobotModule.Pika();
        
        DiscordInteractionMock.Verify(x => x.RespondWithFileAsync(
            "./Images/pika.jpeg",
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null,
            null,
            null,
            MessageFlags.None
        ), Times.Once);
    }
}
