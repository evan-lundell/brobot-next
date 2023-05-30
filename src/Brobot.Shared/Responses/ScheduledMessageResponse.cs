namespace Brobot.Shared.Responses;

public class ScheduledMessageResponse
{
    public int Id { get; set; }
    public required string MessageText { get; set; }
    public DateTime? SendDate { get; set; }
    public DateTime? SentDate { get; set; }
    public ulong ChannelId { get; set; }
    public ulong CreatedById { get; set; }
}