namespace Brobot.Models;

public class DailyMessageCountModel
{
    public virtual required UserModel User { get; set; }
    public ulong UserId { get; set; }
    
    public virtual required ChannelModel Channel { get; set; }
    public ulong ChannelId { get; set; }

    public required DateOnly CountDate { get; set; }

    public int MessageCount { get; set; }
}