namespace Brobot.Shared.Requests;

public record GenerateWordCloudRequest
{
    public ulong ChannelId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IncludeMessageCount { get; set; } = true;
}