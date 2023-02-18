namespace Brobot.Models;

public class ChannelModel
{
    public ulong Id { get; set; }
    public required string Name { get; set; }
    public bool Archived { get; set; }

    public ulong GuildId { get; set; }
    public required GuildModel Guild { get; set; }

    public virtual ICollection<ChannelUserModel> ChannelUsers { get; set; }
    public virtual ICollection<ScheduledMessageModel> ScheduledMessages { get; set; }

    public ChannelModel()
    {
        ChannelUsers = new HashSet<ChannelUserModel>();
        ScheduledMessages = new HashSet<ScheduledMessageModel>();
    }
}