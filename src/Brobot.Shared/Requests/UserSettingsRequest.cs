namespace Brobot.Shared.Requests;

public record UserSettingsRequest
{
    public string? Timezone { get; init; }
    public ulong? PrimaryChannelId { get; init; }
    public DateOnly? BirthDate { get; set; }
}