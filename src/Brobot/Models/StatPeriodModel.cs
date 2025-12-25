namespace Brobot.Models;

public class StatPeriodModel
{
    public int Id { get; set; }

    public required ChannelModel Channel { get; set; }
    public ulong ChannelId { get; set; }
    
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    
    public virtual ICollection<WordCountModel> WordCounts { get; set; } = new HashSet<WordCountModel>();
    public virtual ICollection<DiscordUserMessageCountModel> UserMessageCounts { get; set; } = new HashSet<DiscordUserMessageCountModel>();
}