using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Brobot.Frontend.Providers;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Handlers;

public class JwtTokenMessageHandler : DelegatingHandler
{
    private readonly Uri _allowedBaseAddress;
    private readonly JwtAuthenticationStateProvider _loginStateService;

    public JwtTokenMessageHandler(Uri allowedBaseAddress, JwtAuthenticationStateProvider loginStateService)
    {
        _allowedBaseAddress = allowedBaseAddress;
        _loginStateService = loginStateService;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return SendAsync(request, cancellationToken).Result;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri;
        var isSelfApiAccess = uri != null && _allowedBaseAddress.IsBaseOf(uri);

        if (isSelfApiAccess)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _loginStateService.Token ?? string.Empty);
        }

        var httpResponseMessage = await base.SendAsync(request, cancellationToken);
        if (httpResponseMessage is { IsSuccessStatusCode: false, StatusCode: HttpStatusCode.InternalServerError })
        {
            var errorResponse = await httpResponseMessage.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: cancellationToken);
            if (errorResponse != null)
            {
                throw new Exception(errorResponse.Title);
            }

            var message = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
            throw new Exception(string.IsNullOrWhiteSpace(message) ? "Something went wrong" : message);
        }

        return httpResponseMessage;
    }
}