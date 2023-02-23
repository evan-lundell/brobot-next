namespace Brobot.Models;

public class HotOpSessionModel
{
    public int Id { get; set; }
    public required UserModel User { get; set; }
    public ulong UserId { get; set; }
    public required HotOpModel HotOp { get; set; }
    public int HotOpId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
}