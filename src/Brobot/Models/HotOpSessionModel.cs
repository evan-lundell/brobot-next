namespace Brobot.Models;

public class HotOpSessionModel
{
    public int Id { get; set; }
    public virtual required UserModel User { get; set; }
    public ulong UserId { get; set; }
    public virtual required HotOpModel HotOp { get; set; }
    public int HotOpId { get; set; }
    public DateTimeOffset StartDateTime { get; set; }
    public DateTimeOffset? EndDateTime { get; set; }
}