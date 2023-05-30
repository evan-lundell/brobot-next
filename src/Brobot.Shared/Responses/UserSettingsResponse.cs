namespace Brobot.Shared.Responses;

public class UserSettingsResponse
{
    public string? Timezone { get; set; }
    public ulong? PrimaryChannelId { get; set; }
    public DateTime? Birthdate { get; set; }
}