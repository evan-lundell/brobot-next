namespace Brobot.Shared.Requests;

public record SendMessageRequest
{
    public ulong ChannelId { get; init; }
    public required string Message { get; init; }
}