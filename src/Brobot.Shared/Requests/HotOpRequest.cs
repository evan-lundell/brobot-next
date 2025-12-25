namespace Brobot.Shared.Requests;

public record HotOpRequest
{
    public int? Id { get; init; }
    public ulong ChannelId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
}