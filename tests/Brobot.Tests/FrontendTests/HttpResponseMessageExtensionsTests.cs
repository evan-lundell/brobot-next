using System.Net;
using System.Text;
using Brobot.Frontend.Services;

namespace Brobot.Tests.FrontendTests;

[TestFixture]
public class HttpResponseMessageExtensionsTests
{
    [Test]
    public async Task ReadErrorMessage_PlainText_ReturnsBody()
    {
        // What BadRequest("...") sends
        using var response = Response(HttpStatusCode.BadRequest, "Start date must be before end date", "text/plain");

        Assert.That(await response.ReadErrorMessage(), Is.EqualTo("Start date must be before end date"));
    }

    [Test]
    public async Task ReadErrorMessage_JsonString_ReturnsString()
    {
        using var response = Response(HttpStatusCode.BadRequest, "\"Invalid channel\"", "application/json");

        Assert.That(await response.ReadErrorMessage(), Is.EqualTo("Invalid channel"));
    }

    [Test]
    public async Task ReadErrorMessage_ProblemDetails_ReturnsTitle()
    {
        using var response = Response(
            HttpStatusCode.BadRequest,
            "{\"type\":\"https://tools.ietf.org/html/rfc9110#section-15.5.1\",\"title\":\"One or more validation errors occurred.\",\"status\":400}",
            "application/problem+json");

        Assert.That(await response.ReadErrorMessage(), Is.EqualTo("One or more validation errors occurred."));
    }

    [Test]
    public async Task ReadErrorMessage_PascalCaseTitle_ReturnsTitle()
    {
        using var response = Response(HttpStatusCode.BadRequest, "{\"Type\":\"error\",\"Title\":\"Channel not found\"}", "application/json");

        Assert.That(await response.ReadErrorMessage(), Is.EqualTo("Channel not found"));
    }

    [Test]
    public async Task ReadErrorMessage_NonStringTitle_ReturnsNull()
    {
        using var response = Response(HttpStatusCode.BadRequest, "{\"title\":42}", "application/json");

        Assert.That(await response.ReadErrorMessage(), Is.Null);
    }

    [TestCase(HttpStatusCode.Forbidden, "You don't have permission to do that")]
    [TestCase(HttpStatusCode.Unauthorized, "You need to log in again")]
    [TestCase(HttpStatusCode.NotFound, null)]
    public async Task ReadErrorMessage_EmptyBody_UsesStatusCode(HttpStatusCode statusCode, string? expected)
    {
        // What Forbid() sends
        using var response = new HttpResponseMessage(statusCode) { Content = new StringContent("") };

        Assert.That(await response.ReadErrorMessage(), Is.EqualTo(expected));
    }

    [TestCase("{\"errors\":{}}", "application/json", TestName = "ReadErrorMessage_JsonObjectWithoutTitle_ReturnsNull")]
    [TestCase("[1,2]", "application/json", TestName = "ReadErrorMessage_JsonArray_ReturnsNull")]
    [TestCase("<html>Bad Gateway</html>", "text/html", TestName = "ReadErrorMessage_Html_ReturnsNull")]
    public async Task ReadErrorMessage_NothingUsable_ReturnsNull(string body, string mediaType)
    {
        using var response = Response(HttpStatusCode.BadGateway, body, mediaType);

        Assert.That(await response.ReadErrorMessage(), Is.Null);
    }

    [Test]
    public async Task ToException_UsesApiMessage()
    {
        using var response = Response(HttpStatusCode.BadRequest, "Invalid user", "text/plain");

        var exception = await response.ToException("Failed to create entity Hot Op");

        Assert.That(exception.Message, Is.EqualTo("Invalid user"));
    }

    [Test]
    public async Task ToException_FallsBackWhenNoMessage()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("") };

        var exception = await response.ToException("Failed to create entity Hot Op");

        Assert.That(exception.Message, Is.EqualTo("Failed to create entity Hot Op"));
    }

    private static HttpResponseMessage Response(HttpStatusCode statusCode, string body, string mediaType) => new(statusCode)
    {
        Content = new StringContent(body, Encoding.UTF8, mediaType)
    };
}
