using Discord;
using Discord.WebSocket;
using Moq;

namespace Brobot.Tests.HotOpServiceTests;

[TestFixture]
public class UserVoiceStateUpdatedTests : HotOpServiceTestsBase
{
    [Test]
    public async Task BotUserIsIgnored()
    {
        var socketUserMock = new Mock<SocketUser>();
        socketUserMock.SetupGet(u => u.IsBot).Returns(true);
        var voiceStateMock = new Mock<IVoiceState>();
        var beforeCount = (await UnitOfWork.HotOpSessions.GetAll()).Count();
        
    }
}