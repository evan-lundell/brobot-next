namespace Brobot.Dtos;

public record StatsDto
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public ulong ChannelId { get; init; }
    public IEnumerable<WordCountDto> WordCounts { get; init; } = [];
    public IEnumerable<MessageCountDto> MessageCounts { get; init; } = [];
}