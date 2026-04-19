using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class RollTests : BrobotModuleTestBase
{
    [Test]
    public async Task InvalidFormat_RespondsWithError()
    {
        // Act
        await BrobotModule.Roll("invalid");
        
        // Assert
        AssertRespondAsyncCalledOnce("Invalid format. Please use #d# (example: 2d6)", true);
    }

    [Test]
    public async Task FirstValueNotNumber_RespondsWithError()
    {
        await BrobotModule.Roll("xd6");
        
        AssertRespondAsyncCalledOnce("Invalid format. Please use #d# (example: 2d6)", true);
    }
    
    [Test]
    public async Task SecondValueNotNumber_RespondsWithError()
    {
        await BrobotModule.Roll("2dx");
        
        AssertRespondAsyncCalledOnce("Invalid format. Please use #d# (example: 2d6)", true);
    }

    [Test]
    public async Task TooManyDice_RespondsWithError()
    {
        await BrobotModule.Roll("26d6");
        
        AssertRespondAsyncCalledOnce("Why do you need to roll so many dice? :thinking:");
    }

    [Test]
    public async Task ValidFormat_RespondsWithSuccess()
    {
        // Sequence with the given seed: 6,1,5,6,3,6,5,4,3,1
        
        await BrobotModule.Roll("10d6");
        
        AssertRespondAsyncCalledOnce("6, 1, 5, 6, 3, 6, 5, 4, 3, 1\nTotal: 40");
    }
}
