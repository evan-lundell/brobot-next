namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class ScheduledMessageModel
{
    public int Id { get; init; }
    public required string MessageText { get; set; }
    public DateTimeOffset? SendDate { get; set; }
    public DateTimeOffset? SentDate { get; set; }

    public virtual required ChannelModel Channel { get; init; }
    public ulong ChannelId { get; set; }

    public virtual required DiscordUserModel CreatedBy { get; init; }
    public ulong CreatedById { get; init; }
}