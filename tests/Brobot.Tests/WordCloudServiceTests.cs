using System.Net;
using Brobot.Dtos;
using Brobot.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Brobot.Tests;

[TestFixture]
public class WordCloudServiceTests
{
    [Test]
    public async Task GetWordCloud_ReturnsBytes_OnSuccess()
    {
        // Arrange
        var expectedBytes = new byte[] { 1, 2, 3 };
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(expectedBytes)
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var loggerMock = new Mock<ILogger<WordCloudService>>();
        var service = new WordCloudService(httpClient, loggerMock.Object);

        var wordCounts = new[] { new WordCountDto { Word = "test", Count = 5 } };

        // Act
        var result = await service.GetWordCloud(wordCounts);

        // Assert
        Assert.That(result, Is.EqualTo(expectedBytes));
    }

    [Test]
    public async Task GetWordCloud_ReturnsEmptyArray_OnException()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var loggerMock = new Mock<ILogger<WordCloudService>>();
        var service = new WordCloudService(httpClient, loggerMock.Object);

        var wordCounts = new[] { new WordCountDto { Word = "fail", Count = 1 } };

        // Act
        var result = await service.GetWordCloud(wordCounts);

        // Assert
        Assert.That(result, Is.Empty);

        // Verify error was logged
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error getting wordcloud")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }
}