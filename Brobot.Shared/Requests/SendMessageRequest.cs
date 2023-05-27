namespace Brobot.Shared.Requests;

public class SendMessageRequest
{
    public ulong ChannelId { get; init; }
    public required string Message { get; init; }
}