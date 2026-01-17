namespace Brobot.Shared.Responses;

public record HotOpResponse
{
    public int Id { get; init; }
    public required DiscordUserResponse DiscordUser { get; init; }
    public required ChannelResponse Channel { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
}