namespace Brobot.Responses;

public class RandomFactResponse
{
    public string? Id { get; set; }
    public string? Text { get; init; }
    public string? Source { get; set; }
    public string? SourceUrl { get; set; }
    public string? Language { get; set; }
    public string? Permalink { get; set; }
}