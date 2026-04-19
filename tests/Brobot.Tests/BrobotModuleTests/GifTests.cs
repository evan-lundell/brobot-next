using Discord;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public class GifTests : BrobotModuleTestBase
{
    [Test]
    public async Task GiphyServiceThrowsError_RespondWithErrorMessage()
    {
        GiphyServiceMock.Setup(g => g.GetGif(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Giphy API error"));
        
        await BrobotModule.Gif("funny cat");
        
        AssertRespondAsyncCalledOnce("An error occurred");
    }
    
    [Test]
    public async Task GiphyServiceReturnsGif_RespondWithGifUrl()
    {
        GiphyServiceMock.Setup(g => g.GetGif("funny cat"))
            .ReturnsAsync("https://giphy.com/funny-cat.gif");
        
        await BrobotModule.Gif("funny cat");
        
        AssertRespondAsyncCalledOnce("https://giphy.com/funny-cat.gif");
    }
}