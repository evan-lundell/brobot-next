using Moq;
using Moq.Protected;
using System.Net;
using Brobot.Services;
using Newtonsoft.Json;

namespace Brobot.Tests;

[TestFixture]
public class DictionaryServiceTests
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _mockHttpClient;
    private DictionaryService _dictionaryService;

    [SetUp]
    public void SetUp()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        _dictionaryService = new DictionaryService(_mockHttpClient);
    }
    
    [TearDown]
    public void TearDown()
    {
        _mockHttpClient.Dispose();
    }

    [Test]
    public async Task GetDefinition_ReturnsCorrectDefinition()
    {
        // Arrange
        var word = "test";
        var expectedDefinition = """
                                 1: (noun) A challenge, trial.
                                 2: (verb) To challenge.
                                 """;
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("""[{"word":"test","phonetic":"/test/","phonetics":[{"text":"/test/","audio":"https://api.dictionaryapi.dev/media/pronunciations/en/test-uk.mp3","sourceUrl":"https://commons.wikimedia.org/w/index.php?curid=9014228","license":{"name":"BY 3.0 US","url":"https://creativecommons.org/licenses/by/3.0/us"}},{"text":"/test/","audio":"https://api.dictionaryapi.dev/media/pronunciations/en/test-us.mp3","sourceUrl":"https://commons.wikimedia.org/w/index.php?curid=1197419","license":{"name":"BY-SA 3.0","url":"https://creativecommons.org/licenses/by-sa/3.0"}}],"meanings":[{"partOfSpeech":"noun","definitions":[{"definition":"A challenge, trial.","synonyms":[],"antonyms":[]},{"definition":"A cupel or cupelling hearth in which precious metals are melted for trial and refinement.","synonyms":[],"antonyms":[]},{"definition":"(academia) An examination, given often during the academic term.","synonyms":[],"antonyms":[]},{"definition":"A session in which a product or piece of equipment is examined under everyday or extreme conditions to evaluate its durability, etc.","synonyms":[],"antonyms":[]},{"definition":"(normally “Test”) A Test match.","synonyms":[],"antonyms":[]},{"definition":"The external calciferous shell, or endoskeleton, of an echinoderm, e.g. sand dollars and sea urchins.","synonyms":[],"antonyms":[]},{"definition":"Testa; seed coat.","synonyms":[],"antonyms":[]},{"definition":"Judgment; distinction; discrimination.","synonyms":[],"antonyms":[]}],"synonyms":["examination","quiz"],"antonyms":["recess"]},{"partOfSpeech":"verb","definitions":[{"definition":"To challenge.","synonyms":[],"antonyms":[],"example":"Climbing the mountain tested our stamina."},{"definition":"To refine (gold, silver, etc.) in a test or cupel; to subject to cupellation.","synonyms":[],"antonyms":[]},{"definition":"To put to the proof; to prove the truth, genuineness, or quality of by experiment, or by some principle or standard; to try.","synonyms":[],"antonyms":[],"example":"to test the soundness of a principle; to test the validity of an argument"},{"definition":"(academics) To administer or assign an examination, often given during the academic term, to (somebody).","synonyms":[],"antonyms":[]},{"definition":"To place a product or piece of equipment under everyday and/or extreme conditions and examine it for its durability, etc.","synonyms":[],"antonyms":[]},{"definition":"To be shown to be by test.","synonyms":[],"antonyms":[],"example":"He tested positive for cancer."},{"definition":"To examine or try, as by the use of some reagent.","synonyms":[],"antonyms":[],"example":"to test a solution by litmus paper"}],"synonyms":[],"antonyms":[]}],"license":{"name":"CC BY-SA 3.0","url":"https://creativecommons.org/licenses/by-sa/3.0"},"sourceUrls":["https://en.wiktionary.org/wiki/test"]},{"word":"test","phonetic":"/test/","phonetics":[{"text":"/test/","audio":"https://api.dictionaryapi.dev/media/pronunciations/en/test-uk.mp3","sourceUrl":"https://commons.wikimedia.org/w/index.php?curid=9014228","license":{"name":"BY 3.0 US","url":"https://creativecommons.org/licenses/by/3.0/us"}},{"text":"/test/","audio":"https://api.dictionaryapi.dev/media/pronunciations/en/test-us.mp3","sourceUrl":"https://commons.wikimedia.org/w/index.php?curid=1197419","license":{"name":"BY-SA 3.0","url":"https://creativecommons.org/licenses/by-sa/3.0"}}],"meanings":[{"partOfSpeech":"noun","definitions":[{"definition":"A witness.","synonyms":[],"antonyms":[]}],"synonyms":[],"antonyms":[]},{"partOfSpeech":"verb","definitions":[{"definition":"To attest (a document) legally, and date it.","synonyms":[],"antonyms":[]},{"definition":"To make a testament, or will.","synonyms":[],"antonyms":[]}],"synonyms":[],"antonyms":[]}],"license":{"name":"CC BY-SA 3.0","url":"https://creativecommons.org/licenses/by-sa/3.0"},"sourceUrls":["https://en.wiktionary.org/wiki/test"]},{"word":"test","phonetic":"/test/","phonetics":[{"text":"/test/","audio":"https://api.dictionaryapi.dev/media/pronunciations/en/test-uk.mp3","sourceUrl":"https://commons.wikimedia.org/w/index.php?curid=9014228","license":{"name":"BY 3.0 US","url":"https://creativecommons.org/licenses/by/3.0/us"}},{"text":"/test/","audio":"https://api.dictionaryapi.dev/media/pronunciations/en/test-us.mp3","sourceUrl":"https://commons.wikimedia.org/w/index.php?curid=1197419","license":{"name":"BY-SA 3.0","url":"https://creativecommons.org/licenses/by-sa/3.0"}}],"meanings":[{"partOfSpeech":"noun","definitions":[{"definition":"(body building) testosterone","synonyms":[],"antonyms":[]}],"synonyms":[],"antonyms":[]}],"license":{"name":"CC BY-SA 3.0","url":"https://creativecommons.org/licenses/by-sa/3.0"},"sourceUrls":["https://en.wiktionary.org/wiki/test"]}]"""),
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var actualDefinition = await _dictionaryService.GetDefinition(word);

        // Assert
        Assert.That(actualDefinition, Is.EqualTo(expectedDefinition));
    }
    
    [Test]
    public async Task GetDefinition_ReturnsErrorMessageForNonExistentWord()
    {
        // Arrange
        var nonExistentWord = "nonexistentword";
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound,
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().EndsWith(nonExistentWord)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _dictionaryService.GetDefinition(nonExistentWord);

        // Assert
        Assert.That(result, Is.EqualTo("That's not a word dummy"));
    }
    
    [Test]
    public async Task GetDefinition_ReturnsErrorMessageForEmptyResponse()
    {
        // Arrange
        var word = "test";
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("[]"), // Empty array to simulate empty response
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().EndsWith(word)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _dictionaryService.GetDefinition(word);

        // Assert
        Assert.That(result, Is.EqualTo("An error occured"));
    }
    
    [Test]
    public async Task GetDefinition_ReturnsErrorMessageForEmptyMeanings()
    {
        // Arrange
        var word = "test";
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("[{\"meanings\":[]}]"), // Empty meanings array to simulate empty meanings
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().EndsWith(word)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _dictionaryService.GetDefinition(word);

        // Assert
        Assert.That(result, Is.EqualTo("An error occured"));
    }
    
    [Test]
    public void GetDefinition_ThrowsJsonReaderExceptionForInvalidJson()
    {
        // Arrange
        var word = "test";
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{invalid json}"), // Invalid JSON to simulate invalid response
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().EndsWith(word)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act & Assert
        Assert.ThrowsAsync<JsonReaderException>(() => _dictionaryService.GetDefinition(word));
    }
    
    [Test]
    public async Task GetDefinition_SkipsMeaningWithNullDefinitions()
    {
        // Arrange
        var word = "test";
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("[{\"meanings\":[{\"partOfSpeech\":\"noun\",\"definitions\":null},{\"partOfSpeech\":\"verb\",\"definitions\":[{\"definition\":\"a test definition\"}]}]}]"), // JSON response with a Meaning object that has a null Definitions property
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().EndsWith(word)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _dictionaryService.GetDefinition(word);

        // Assert
        Assert.That(result, Is.EqualTo("1: (verb) a test definition"));
    }
}