namespace Brobot.Dtos;

public record WordCountDto
{
    public ulong ChannelId { get; set; }
    public required string Word { get; set; }
    public int Count { get; set; }
}