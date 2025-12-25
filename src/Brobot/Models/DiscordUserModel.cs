// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable CollectionNeverUpdated.Global

namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class DiscordUserModel
{
    public ulong Id { get; init; }
    public required string Username { get; set; }
    public DateOnly? Birthdate { get; set; }
    public string? Timezone { get; set; }
    public DateTimeOffset? LastOnline { get; set; }
    public bool Archived { get; set; }

    public virtual ChannelModel? PrimaryChannel { get; init; }
    public ulong? PrimaryChannelId { get; set; }

    public virtual ICollection<GuildDiscordUserModel> GuildUsers { get; init; }
    public virtual ICollection<ChannelDiscordUserModel> ChannelUsers { get; init; }
    public virtual ICollection<ScheduledMessageModel> ScheduledMessages { get; init; }
    public virtual ICollection<HotOpModel> HotOps { get; init; }
    // ReSharper disable once CollectionNeverUpdated.Global
    public virtual ICollection<HotOpSessionModel> HotOpSessions { get; init; }

    public virtual ICollection<DailyMessageCountModel> DailyCounts { get; init; }

    public virtual ICollection<SecretSantaGroupDiscordUserModel> SecretSantaGroupUsers { get; init; }
    public virtual ICollection<SecretSantaPairModel> Givers { get; init; }
    public virtual ICollection<SecretSantaPairModel> Recipients { get; init; }

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