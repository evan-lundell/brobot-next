namespace Brobot.Models;

public class HotOpSessionModel
{
    public int Id { get; set; }
    public virtual required DiscordUserModel DiscordUser { get; set; }
    public ulong DiscordUserId { get; set; }
    public virtual required HotOpModel HotOp { get; set; }
    public int HotOpId { get; set; }
    public DateTimeOffset StartDateTime { get; set; }
    public DateTimeOffset? EndDateTime { get; set; }
}