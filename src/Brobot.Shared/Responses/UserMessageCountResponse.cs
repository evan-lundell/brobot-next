namespace Brobot.Shared.Responses;

public record UserMessageCountResponse
{
    public ulong UserId { get; init; }
    public int Count { get; set; }
}