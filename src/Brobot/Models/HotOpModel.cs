// ReSharper disable VirtualMemberCallInConstructor
namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class HotOpModel
{
    public int Id { get; init; }
    public virtual required DiscordUserModel DiscordUser { get; init; }
    public ulong UserId { get; init; }
    public virtual required ChannelModel Channel { get; set; }
    public ulong ChannelId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public virtual ICollection<HotOpSessionModel> HotOpSessions { get; init; }

    public HotOpModel()
    {
        HotOpSessions = new HashSet<HotOpSessionModel>();
    }
}