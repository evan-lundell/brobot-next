namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class HotOpSessionModel
{
    public int Id { get; init; }
    public virtual required DiscordUserModel DiscordUser { get; init; }
    public ulong DiscordUserId { get; init; }
    public virtual required HotOpModel HotOp { get; init; }
    public int HotOpId { get; init; }
    public DateTimeOffset StartDateTime { get; init; }
    public DateTimeOffset? EndDateTime { get; set; }
}