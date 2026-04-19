using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class GameTests : BrobotModuleTestBase
{
    [Test]
    public async Task LessThanTwoGames_RespondsWithErrorMessage()
    {
        await BrobotModule.Game("Game1");
        
        AssertRespondAsyncCalledOnce("Please provide at least 2 games", true);
    }
    
    [Test]
    public async Task ValidGameList_RespondsWithRandomGame()
    {
        await BrobotModule.Game("Game1", "Game2", "Game3", "Game4", "Game5", "Game6", "Game7", "Game8", "Game9", "Game10");
        
        AssertRespondAsyncCalledOnce("Let's play Game10");
    }    
}
