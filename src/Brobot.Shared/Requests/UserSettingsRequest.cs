namespace Brobot.Shared.Requests;

public class UserSettingsRequest
{
    public string? Timezone { get; init; }
    public ulong? PrimaryChannelId { get; init; }
    public DateTime? BirthDate { get; set; }
}