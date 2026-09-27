using Brobot.Models;
using Brobot.Repositories;
using Discord;
using Moq;

namespace Brobot.Tests.HotOpModuleTests;

[TestFixture]
public class ScoreboardTests : HotOpModuleTestBase
{
    [Test]
    public async Task ThrowsException_RespondsWithErrorMessage()
    {
        UnitOfWorkMock.Setup(uow => uow.HotOps)
            .Throws(new Exception("Database error"));

        await HotOpModule.Scoreboard();
        
        AssertRespondAsyncCalledOnce("Failed to get hot op scores", true);
    }

    [Test]
    public async Task NoActiveHotOps_RespondsWithNoActiveHotOpsMessage()
    {
        const ulong channelId = 1;
        Mock<ITextChannel> channelMock = new();
        channelMock.SetupGet(c => c.Id).Returns(channelId);
        InteractionContextMock.SetupGet(c => c.Channel).Returns(channelMock.Object);
        Mock<IHotOpRepository> hotOpRepositoryMock = new();
        hotOpRepositoryMock.Setup(repo => repo.GetActiveHotOpsWithSessions(channelId))
            .ReturnsAsync([]);
        UnitOfWorkMock.SetupGet(uow => uow.HotOps)
            .Returns(hotOpRepositoryMock.Object);
        
        await HotOpModule.Scoreboard();
        
        AssertRespondAsyncCalledOnce("No active hot ops");
    }

    [Test]
    public async Task HotOpFound_RespondsWithHotOpScoreboard()
    {
        const ulong channelId = 1;
        Mock<ITextChannel> channelMock = new();
        channelMock.SetupGet(c => c.Id).Returns(channelId);
        InteractionContextMock.SetupGet(c => c.Channel).Returns(channelMock.Object);
        Mock<IHotOpRepository> hotOpRepositoryMock = new();
        List<HotOpModel> hotOpModels =
        [
            new HotOpModel
            {
                Id = 1,
                Channel = new ChannelModel
                {
                    Id = channelId,
                    Name = "TestChannel",
                    Guild = new GuildModel
                    {
                        Name = "TestGuild",
                        Id = 1
                    }
                },
                DiscordUser = new DiscordUserModel
                {
                    Id = 1,
                    Username = "TestUser"
                }
            }
        ];
        hotOpRepositoryMock.Setup(repo => repo.GetActiveHotOpsWithSessions(channelId))
            .ReturnsAsync(hotOpModels);
        UnitOfWorkMock.SetupGet(uow => uow.HotOps)
            .Returns(hotOpRepositoryMock.Object);
        Embed embed = new EmbedBuilder().WithTitle("Hot Op Scoreboard").Build();
        HotOpServiceMock.Setup(ho => ho.CreateScoreboardEmbed(hotOpModels[0]))
            .Returns(embed);
        
        await HotOpModule.Scoreboard();

        DiscordInteractionMock.Verify(x => x.RespondAsync(
            null,
            new Embed[] { embed },
            false,
            false,
            null,
            null,
            null,
            null,
            null,
            MessageFlags.None), Times.Once);
    }
}
