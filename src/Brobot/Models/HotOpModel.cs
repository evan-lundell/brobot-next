// ReSharper disable VirtualMemberCallInConstructor
namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class HotOpModel
{
    public int Id { get; set; }
    public virtual required UserModel User { get; set; }
    public ulong UserId { get; set; }
    public virtual required ChannelModel Channel { get; set; }
    public ulong ChannelId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public virtual ICollection<HotOpSessionModel> HotOpSessions { get; set; }

    public HotOpModel()
    {
        HotOpSessions = new HashSet<HotOpSessionModel>();
    }
}