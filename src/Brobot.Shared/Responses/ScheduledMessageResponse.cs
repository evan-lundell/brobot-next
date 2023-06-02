namespace Brobot.Shared.Responses;

public record ScheduledMessageResponse
{
    public int Id { get; init; }
    public required string MessageText { get; set; }
    public DateTimeOffset? SendDate { get; set; }
    public DateTimeOffset? SentDate { get; set; }
    public required ChannelResponse Channel { get; set; }
    public ulong CreatedById { get; set; }
}