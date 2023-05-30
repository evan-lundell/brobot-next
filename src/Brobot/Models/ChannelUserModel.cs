namespace Brobot.Models;

public class ChannelUserModel
{
    public required ChannelModel Channel { get; set; }
    public ulong ChannelId { get; set; }

    public required UserModel User { get; set; }
    public ulong UserId { get; set; }
}