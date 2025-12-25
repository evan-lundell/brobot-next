namespace Brobot.Dtos;

public record WordCountDto
{
    public required string Word { get; init; }
    public int Count { get; set; }
}