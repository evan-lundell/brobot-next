// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable CollectionNeverUpdated.Global

namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class DiscordUserModel
{
    public ulong Id { get; set; }
    public required string Username { get; set; }
    public DateOnly? Birthdate { get; set; }
    public string? Timezone { get; set; }
    public DateTimeOffset? LastOnline { get; set; }
    public bool Archived { get; set; }

    public virtual ChannelModel? PrimaryChannel { get; set; }
    public ulong? PrimaryChannelId { get; set; }

    public virtual ICollection<GuildDiscordUserModel> GuildUsers { get; set; }
    public virtual ICollection<ChannelDiscordUserModel> ChannelUsers { get; set; }
    public virtual ICollection<ScheduledMessageModel> ScheduledMessages { get; set; }
    public virtual ICollection<HotOpModel> HotOps { get; set; }
    // ReSharper disable once CollectionNeverUpdated.Global
    public virtual ICollection<HotOpSessionModel> HotOpSessions { get; set; }

    public virtual ICollection<DailyMessageCountModel> DailyCounts { get; set; }

    public virtual ICollection<SecretSantaGroupDiscordUserModel> SecretSantaGroupUsers { get; set; }
    public virtual ICollection<SecretSantaPairModel> Givers { get; set; }
    public virtual ICollection<SecretSantaPairModel> Recipients { get; set; }

    public DiscordUserModel()
    {
        GuildUsers = new HashSet<GuildDiscordUserModel>();
        ChannelUsers = new HashSet<ChannelDiscordUserModel>();
        ScheduledMessages = new HashSet<ScheduledMessageModel>();
        HotOps = new HashSet<HotOpModel>();
        HotOpSessions = new HashSet<HotOpSessionModel>();
        DailyCounts = new HashSet<DailyMessageCountModel>();
        SecretSantaGroupUsers = new HashSet<SecretSantaGroupDiscordUserModel>();
        Givers = new HashSet<SecretSantaPairModel>();
        Recipients = new HashSet<SecretSantaPairModel>();
    }
}