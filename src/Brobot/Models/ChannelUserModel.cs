namespace Brobot.Models;

public class ChannelUserModel
{
    public virtual required ChannelModel Channel { get; set; }
    public ulong ChannelId { get; set; }

    public virtual required UserModel User { get; set; }
    public ulong UserId { get; set; }
}