namespace Brobot.Shared.Responses;

public record ScheduledMessageResponse
{
    public int Id { get; init; }
    public required string MessageText { get; set; }
    public DateTime? SendDate { get; set; }
    public DateTime? SentDate { get; set; }
    public required ChannelResponse Channel { get; set; }
    public ulong CreatedById { get; set; }
}