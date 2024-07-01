namespace Brobot.Shared.Requests;

public record WordCloudRequest
{
    public ulong ChannelId { get; init; }
    public int MonthsBack { get; init; }
}