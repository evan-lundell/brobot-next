using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class TeamsTests : BrobotModuleTestBase
{
    [Test]
    public async Task LessThanTwoPlayers_RespondsWtihErrorMessage()
    {
        await BrobotModule.Teams();
        
        AssertRespondAsyncCalledOnce("Please provide at least 2 players", true);
    }

    [Test]
    public async Task ValidPlayers_RespondsWtihSuccessMessage()
    {
        await BrobotModule.Teams(
            "Player1", "Player2", "Player3", "Player4", "Player5", "Player6", "Player7",  "Player8", "Player9", "Player10");
        
        AssertRespondAsyncCalledOnce("Player10, Player7, Player4, Player5, Player1\nPlayer2, Player9, Player8, Player3, Player6");
    }
}