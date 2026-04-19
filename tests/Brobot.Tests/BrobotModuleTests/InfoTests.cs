using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class InfoTests : BrobotModuleTestBase
{
    [Test]
    public async Task RespondsWithInfo()
    {
        // Assert
        const ulong guildId = 1;
        const ulong channelId = 2;
        const ulong userId = 3;
        Mock<IGuild>  mockGuild = new Mock<IGuild>();
        mockGuild.SetupGet(x => x.Id).Returns(guildId);
        Mock<IMessageChannel> mockMessageChannel = new Mock<IMessageChannel>();
        mockMessageChannel.SetupGet(x => x.Id).Returns(channelId);
        Mock<IUser>  mockUser = new Mock<IUser>();
        mockUser.SetupGet(x => x.Id).Returns(userId);
        InteractionContextMock.SetupGet(x => x.Guild)
            .Returns(mockGuild.Object);
        InteractionContextMock.SetupGet(x => x.Channel)
            .Returns(mockMessageChannel.Object);
        InteractionContextMock.SetupGet(x => x.User)
            .Returns(mockUser.Object);
        string expectedResponse = $"Guild: {guildId}\nChannel: {channelId}\nUser: {userId}";
        
        // Act
        await BrobotModule.Info();
        
        // Assert
        AssertRespondAsyncCalledOnce(expectedResponse);
    }
}