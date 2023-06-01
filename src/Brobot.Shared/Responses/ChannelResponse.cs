namespace Brobot.Shared.Responses;

public record ChannelResponse
{
    public ulong Id { get; init; }
    public required string Name { get; init; }
}