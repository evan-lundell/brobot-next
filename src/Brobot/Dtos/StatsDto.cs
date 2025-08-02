namespace Brobot.Dtos;

public record StatsDto
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public ulong ChannelId { get; set; }
    public IEnumerable<WordCountDto> WordCounts { get; set; } = [];
    public IEnumerable<MessageCountDto> MessageCounts { get; set; } = [];
}