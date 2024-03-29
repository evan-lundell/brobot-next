namespace Brobot.Shared.Responses;

public record UserSettingsResponse
{
    public string? Timezone { get; set; }
    public ulong? PrimaryChannelId { get; set; }
    public DateOnly? Birthdate { get; set; }
}