namespace Brobot.Shared.Responses;

public record WordCloudResponse
{
    public required byte[] ImageBytes { get; set; } 
}