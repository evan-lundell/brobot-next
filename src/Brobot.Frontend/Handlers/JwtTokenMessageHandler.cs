using System.Net;
using System.Net.Http.Headers;
using Brobot.Frontend.Providers;
using Brobot.Frontend.Services;

namespace Brobot.Frontend.Handlers;

public class JwtTokenMessageHandler(Uri allowedBaseAddress, JwtAuthenticationStateProvider loginStateService)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri;
        var isSelfApiAccess = uri != null && allowedBaseAddress.IsBaseOf(uri);

        if (isSelfApiAccess)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginStateService.Token ?? string.Empty);
        }

        var httpResponseMessage = await base.SendAsync(request, cancellationToken);
        if (httpResponseMessage is { IsSuccessStatusCode: false, StatusCode: HttpStatusCode.InternalServerError })
        {
            throw await httpResponseMessage.ToException("Something went wrong");
        }

        return httpResponseMessage;
    }
}