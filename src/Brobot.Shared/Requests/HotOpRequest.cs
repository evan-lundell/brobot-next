namespace Brobot.Shared.Requests;

public record HotOpRequest
{
    public int? Id { get; set; }
    public ulong ChannelId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
}