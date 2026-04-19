using Brobot.Modules;
using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class DohTests : BrobotModuleTestBase
{
    [Test]
    public async Task DohCalled_RespondsWithPikaFile()
    {
        await BrobotModule.Doh();
        
        DiscordInteractionMock.Verify(x => x.RespondWithFileAsync(
            "./Images/doh.png",
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
