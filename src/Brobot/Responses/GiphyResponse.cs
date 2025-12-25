// ReSharper disable ClassNeverInstantiated.Global
namespace Brobot.Responses;

public class GiphyResponse
{
    public GiphyData? Data { get; init; }
}

public class GiphyData
{
    public string? Url { get; init; }
}