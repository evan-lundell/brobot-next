namespace Brobot.Dtos;

public record MessageCountDto
{
    public ulong UserId { get; init; }
    public required string Username { get; init; }
    public int Count { get; init; }
}