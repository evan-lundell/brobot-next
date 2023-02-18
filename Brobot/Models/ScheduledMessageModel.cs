namespace Brobot.Models;

public class ScheduledMessageModel
{
    public int Id { get; set; }
    public required string MessageText { get; set; }
    public DateTime? SendDate { get; set; }
    public DateTime? SentDate { get; set; }

    public required ChannelModel Channel { get; set; }
    public ulong ChannelId { get; set; }

    public required UserModel CreatedBy { get; set; }
    public ulong CreatedById { get; set; }
}