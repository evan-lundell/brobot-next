namespace Brobot.Shared.Responses;

public record MonthlyWordCloudResponse
{
    public required byte[] Image { get; init; }
    public required Dictionary<string, int> UserMessageCounts { get; init; }
}