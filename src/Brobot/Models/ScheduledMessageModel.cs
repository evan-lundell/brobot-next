namespace Brobot.Models;

public class ScheduledMessageModel
{
    public int Id { get; set; }
    public required string MessageText { get; set; }
    public DateTimeOffset? SendDate { get; set; }
    public DateTimeOffset? SentDate { get; set; }

    public virtual required ChannelModel Channel { get; set; }
    public ulong ChannelId { get; set; }

    public virtual required UserModel CreatedBy { get; set; }
    public ulong CreatedById { get; set; }
}