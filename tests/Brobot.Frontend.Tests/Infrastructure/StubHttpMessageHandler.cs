using System.Net;
using System.Text;

namespace Brobot.Frontend.Tests.Infrastructure;

/// <summary>
/// A configurable <see cref="HttpMessageHandler"/> for driving the frontend service layer
/// without a real server. Records every request (and its body) and returns whatever the
/// supplied responder produces.
/// </summary>
public sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];
    public List<string> RequestBodies { get; } = [];

    public HttpRequestMessage LastRequest => Requests[^1];
    public string LastRequestBody => RequestBodies[^1];

    /// <summary>Always responds with the given status and JSON body.</summary>
    public static StubHttpMessageHandler Json(HttpStatusCode statusCode, string json)
        => new(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);
        RequestBodies.Add(request.Content == null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken));
        return responder(request);
    }
}
