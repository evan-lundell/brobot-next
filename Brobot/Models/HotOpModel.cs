namespace Brobot.Models;

public class HotOpModel
{
    public int Id { get; set; }
    public required UserModel User { get; set; }
    public ulong UserId { get; set; }
    public required ChannelModel Channel { get; set; }
    public ulong ChannelId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ICollection<HotOpSessionModel> HotOpSessions { get; set; }

    public HotOpModel()
    {
        HotOpSessions = new HashSet<HotOpSessionModel>();
    }
}