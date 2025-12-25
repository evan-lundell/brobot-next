namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class StatPeriodModel
{
    public int Id { get; init; }

    public required ChannelModel Channel { get; init; }
    public ulong ChannelId { get; init; }
    
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    
    public virtual ICollection<WordCountModel> WordCounts { get; init; } = new HashSet<WordCountModel>();
    public virtual ICollection<DiscordUserMessageCountModel> UserMessageCounts { get; init; } = new HashSet<DiscordUserMessageCountModel>();
}