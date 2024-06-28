using Microsoft.AspNetCore.Identity;

// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable CollectionNeverUpdated.Global

namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class UserModel
{
    public ulong Id { get; set; }
    public required string Username { get; set; }
    public DateOnly? Birthdate { get; set; }
    public string? Timezone { get; set; }
    public DateTimeOffset? LastOnline { get; set; }
    public bool Archived { get; set; }

    public ChannelModel? PrimaryChannel { get; set; }
    public ulong? PrimaryChannelId { get; set; }

    public virtual ICollection<GuildUserModel> GuildUsers { get; set; }
    public virtual ICollection<ChannelUserModel> ChannelUsers { get; set; }
    public virtual ICollection<ScheduledMessageModel> ScheduledMessages { get; set; }
    public virtual ICollection<HotOpModel> HotOps { get; set; }
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<HotOpSessionModel> HotOpSessions { get; set; }

    public IdentityUser? IdentityUser { get; set; }
    public string? IdentityUserId { get; set; }

    public virtual ICollection<DailyMessageCountModel> DailyCounts { get; set; }
    public virtual ICollection<PlaylistModel> Playlists { get; set; }

    public ICollection<SecretSantaGroupUserModel> SecretSantaGroupUsers { get; set; }
    public ICollection<SecretSantaPairModel> Givers { get; set; }
    public ICollection<SecretSantaPairModel> Recipients { get; set; }

    public UserModel()
    {
        GuildUsers = new HashSet<GuildUserModel>();
        ChannelUsers = new HashSet<ChannelUserModel>();
        ScheduledMessages = new HashSet<ScheduledMessageModel>();
        HotOps = new HashSet<HotOpModel>();
        HotOpSessions = new HashSet<HotOpSessionModel>();
        DailyCounts = new HashSet<DailyMessageCountModel>();
        Playlists = new HashSet<PlaylistModel>();
        SecretSantaGroupUsers = new HashSet<SecretSantaGroupUserModel>();
        Givers = new HashSet<SecretSantaPairModel>();
        Recipients = new HashSet<SecretSantaPairModel>();
    }
}