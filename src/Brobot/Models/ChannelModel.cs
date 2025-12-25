// ReSharper disable VirtualMemberCallInConstructor
namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class ChannelModel
{
    public ulong Id { get; init; }
    public required string Name { get; set; }
    public bool Archived { get; set; }
    public bool MonthlyWordCloud { get; init; }
    public string Timezone { get; init; } = "america/chicago";
    
    public ulong GuildId { get; init; }
    public virtual required GuildModel Guild { get; init; }

    public virtual ICollection<ChannelDiscordUserModel> ChannelUsers { get; init; }
    public virtual ICollection<ScheduledMessageModel> ScheduledMessages { get; init; }
    public virtual ICollection<HotOpModel> HotOps { get; init; }
    public virtual ICollection<DailyMessageCountModel> DailyMessageCounts { get; init; }
    public virtual ICollection<StatPeriodModel> StatPeriods { get; init; }

    // This is a collection of users where the given channel is their primary channel
    public virtual ICollection<DiscordUserModel> DiscordUsers { get; init; }
    public ChannelModel()
    {
        ChannelUsers = new HashSet<ChannelDiscordUserModel>();
        ScheduledMessages = new HashSet<ScheduledMessageModel>();
        HotOps = new HashSet<HotOpModel>();
        DiscordUsers = new HashSet<DiscordUserModel>();
        DailyMessageCounts = new HashSet<DailyMessageCountModel>();
        StatPeriods = new HashSet<StatPeriodModel>();
    }
}