using System.Net;
using System.Text.Json;

namespace Brobot.Frontend.Services;

public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Reads a user-facing message from an error response. The API returns errors in several shapes:
    /// plain text from BadRequest("..."), a JSON string, ProblemDetails/ErrorResponse JSON with a title,
    /// or an empty body (e.g. Forbid()). Returns null when there's nothing usable, so callers can fall back
    /// to their own message instead of surfacing a JSON parsing error.
    /// </summary>
    public static async Task<string?> ReadErrorMessage(this HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return response.StatusCode switch
            {
                HttpStatusCode.Forbidden => "You don't have permission to do that",
                HttpStatusCode.Unauthorized => "You need to log in again",
                _ => null
            };
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType == "text/plain")
        {
            return body;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            return root.ValueKind switch
            {
                JsonValueKind.String => root.GetString(),
                JsonValueKind.Object => GetTitle(root),
                _ => null
            };
        }
        catch (JsonException)
        {
            // Not JSON and not labelled as plain text (e.g. an HTML error page); don't show it
            return null;
        }
    }

    private static string? GetTitle(JsonElement root)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, "title", StringComparison.OrdinalIgnoreCase))
            {
                return property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
            }
        }

        return null;
    }

    /// <summary>
    /// Builds the exception to throw for a failed response, using the API's message when there is one.
    /// </summary>
    public static async Task<Exception> ToException(this HttpResponseMessage response, string fallbackMessage)
        => new(await response.ReadErrorMessage() ?? fallbackMessage);
}
