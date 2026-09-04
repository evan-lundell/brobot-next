using System.Net;
using System.Text.Json;
using Brobot.Frontend.Services;
using Brobot.Frontend.Tests.Infrastructure;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Tests.Services;

[TestFixture]
public class HotOpServiceTests
{
    private static HttpClient CreateClient(StubHttpMessageHandler handler)
        => new(handler) { BaseAddress = new Uri("https://localhost/") };

    private static string SerializeHotOps(params HotOpResponse[] hotOps)
        => JsonSerializer.Serialize(hotOps);

    private static HotOpResponse SampleHotOp(int id = 1) => new()
    {
        Id = id,
        DiscordUser = new DiscordUserResponse { Id = 5, Username = "owner" },
        Channel = new ChannelResponse { Id = 10, Name = "general" },
        StartDate = new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.FromHours(-5)),
        EndDate = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.FromHours(-5))
    };

    [Test]
    public async Task GetUpcomingHotOps_RequestsUpcomingType()
    {
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, SerializeHotOps(SampleHotOp()));
        var service = new HotOpService(CreateClient(handler));

        var result = (await service.GetUpcomingHotOps()).ToList();

        Assert.That(handler.LastRequest.Method, Is.EqualTo(HttpMethod.Get));
        Assert.That(handler.LastRequest.RequestUri!.PathAndQuery, Is.EqualTo("/api/HotOps?type=Upcoming"));
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetUpcomingHotOps_PreservesTimezoneOffsetFromResponse()
    {
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, SerializeHotOps(SampleHotOp()));
        var service = new HotOpService(CreateClient(handler));

        var result = (await service.GetUpcomingHotOps()).ToList();

        // The offset the server sent must survive round-tripping so the UI renders local time.
        Assert.That(result[0].StartDate.Offset, Is.EqualTo(TimeSpan.FromHours(-5)));
        Assert.That(result[0].StartDate.Hour, Is.EqualTo(8));
    }

    [Test]
    public async Task Create_PostsToBaseUrlWithRequestBody()
    {
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, JsonSerializer.Serialize(SampleHotOp()));
        var service = new HotOpService(CreateClient(handler));
        var request = new HotOpRequest
        {
            ChannelId = 10,
            StartDate = new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero)
        };

        await service.Create(request);

        Assert.That(handler.LastRequest.Method, Is.EqualTo(HttpMethod.Post));
        Assert.That(handler.LastRequest.RequestUri!.PathAndQuery, Is.EqualTo("/api/HotOps"));
        Assert.That(handler.LastRequestBody, Does.Contain("\"channelId\":10"));
    }

    [Test]
    public void GetUpcomingHotOps_OnErrorStatus_ThrowsWithErrorResponseTitle()
    {
        var error = JsonSerializer.Serialize(new ErrorResponse { Type = "error", Title = "Boom" });
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.BadRequest, error);
        var service = new HotOpService(CreateClient(handler));

        var ex = Assert.ThrowsAsync<Exception>(async () => await service.GetUpcomingHotOps());
        Assert.That(ex!.Message, Is.EqualTo("Boom"));
    }

    [Test]
    public void Delete_OnErrorStatus_ThrowsWithFallbackMessage()
    {
        // JSON null body -> ErrorResponse deserializes to null -> fallback message used.
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.InternalServerError, "null");
        var service = new HotOpService(CreateClient(handler));

        var ex = Assert.ThrowsAsync<Exception>(async () => await service.Delete(1));
        Assert.That(ex!.Message, Does.Contain("Hot Op"));
    }
}
