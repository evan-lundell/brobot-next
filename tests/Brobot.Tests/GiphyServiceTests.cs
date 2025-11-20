using System.Net;
using Brobot.Configuration;
using Brobot.Responses;
using Brobot.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace Brobot.Tests;

[TestFixture]
public class GiphyServiceTests
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _mockHttpClient;
    private GiphyService _giphyService;
    
    [SetUp]
    public void Setup()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        ExternalApisOptions options = new()
        {
            GiphyBaseUrl = "https://localhost",
            GiphyApiKey = "key",
            DictionaryBaseUrl = "https://localhost",
            QuickChartBaseUrl = "https://localhost",
            RandomFactBaseUrl = "https://localhost"
        };
        ILogger<GiphyService> logger = new Mock<ILogger<GiphyService>>().Object;
        _giphyService = new GiphyService(_mockHttpClient, Options.Create(options), logger);
    }
    
    [TearDown]
    public void TearDown()
    {
        _mockHttpClient.Dispose();
    }
    
    [Test]
    public async Task GetGif_ReturnsGifUrl()
    {
        // Arrange
        var tag = "test";
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(new GiphyResponse
            {
                Data = new GiphyData
                {
                    Url = "http://localhost/test.gif"
                }
            }))
        };
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        
        // Act
        var result = await _giphyService.GetGif(tag);
        
        // Assert
        Assert.That(result, Is.EqualTo("http://localhost/test.gif"));
    }
    
    [Test]
    public async Task GetGif_NoTag_ReturnsGifUrl()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(new GiphyResponse
            {
                Data = new GiphyData
                {
                    Url = "http://localhost/test.gif"
                }
            }))
        };
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        
        // Act
        var result = await _giphyService.GetGif(null);
        
        // Assert
        Assert.That(result, Is.EqualTo("http://localhost/test.gif"));
    }
    
    [Test]
    public async Task GetGif_NoData_ReturnsEmptyString()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(new GiphyResponse()))
        };
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        
        // Act
        var result = await _giphyService.GetGif(null);
        
        // Assert
        Assert.That(result, Is.EqualTo(""));
    }
    
    [Test]
    public void GetGif_RequestFails_ThrowsException()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        
        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => await _giphyService.GetGif(null));
    }
}