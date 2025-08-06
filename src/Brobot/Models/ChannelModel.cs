// ReSharper disable VirtualMemberCallInConstructor
namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class ChannelModel
{
    public ulong Id { get; set; }
    public required string Name { get; set; }
    public bool Archived { get; set; }
    public bool MonthlyWordCloud { get; set; }
    public string Timezone { get; set; } = "america/chicago";
    
    public ulong GuildId { get; set; }
    public virtual required GuildModel Guild { get; set; }

    public virtual ICollection<ChannelUserModel> ChannelUsers { get; set; }
    public virtual ICollection<ScheduledMessageModel> ScheduledMessages { get; set; }
    public virtual ICollection<HotOpModel> HotOps { get; set; }
    public virtual ICollection<DailyMessageCountModel> DailyMessageCounts { get; set; }
    public virtual ICollection<StatPeriodModel> StatPeriods { get; set; }

    // This is a collection of users where the given channel is their primary channel
    public virtual ICollection<UserModel> Users { get; set; }
    public ChannelModel()
    {
        ChannelUsers = new HashSet<ChannelUserModel>();
        ScheduledMessages = new HashSet<ScheduledMessageModel>();
        HotOps = new HashSet<HotOpModel>();
        Users = new HashSet<UserModel>();
        DailyMessageCounts = new HashSet<DailyMessageCountModel>();
        StatPeriods = new HashSet<StatPeriodModel>();
    }
}