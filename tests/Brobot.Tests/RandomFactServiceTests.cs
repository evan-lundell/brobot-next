using System.Net;
using Brobot.Responses;
using Brobot.Services;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace Brobot.Tests;

[TestFixture]
public class RandomFactServiceTests
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _mockHttpClient;
    private RandomFactService _randomFactService;
    
    [SetUp]
    public void Setup()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        _randomFactService = new RandomFactService(_mockHttpClient);
    }
    
    [Test]
    public async Task GetFact_ReturnsFact()
    {
        var fact = new RandomFactResponse { Text = "Fact" };
        var json = JsonConvert.SerializeObject(fact);
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });
        
        var result = await _randomFactService.GetFact();
        
        Assert.That(result, Is.EqualTo("Fact"));
    }
    
    [Test]
    public void GetFact_ThrowsExceptionOnFailedRequest()
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });
        
        Assert.ThrowsAsync<HttpRequestException>(async () => await _randomFactService.GetFact());
    }
    
    [Test]
    public async Task GetFact_ReturnsEmptyStringOnNullResponse()
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });
        
        var result = await _randomFactService.GetFact();
        
        Assert.That(result, Is.EqualTo(""));
    }
    
    [Test]
    public void GetFact_ThrowsExceptionOnInvalidJson()
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Invalid JSON")
            });
        
        Assert.ThrowsAsync<JsonReaderException>(async () => await _randomFactService.GetFact());
    }
    
    [TearDown]
    public void TearDown()
    {
        _mockHttpClient.Dispose();
    }
}