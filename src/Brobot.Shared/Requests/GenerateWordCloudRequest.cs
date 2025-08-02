namespace Brobot.Shared.Requests;

public record GenerateWordCloudRequest
{
    public ulong ChannelId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}