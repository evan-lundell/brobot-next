namespace Brobot.Models;

public class UserModel
{
    public ulong Id { get; set; }
    public required string Username { get; set; }
    public DateTime? Birthdate { get; set; }
    public string? Timezone { get; set; }
    public DateTime? LastOnline { get; set; }
    public bool Archived { get; set; }

    public ICollection<GuildUserModel> GuildUsers { get; set; }
    public ICollection<ChannelUserModel> ChannelUsers { get; set; }
    public ICollection<ScheduledMessageModel> ScheduledMessages { get; set; }

    public UserModel()
    {
        GuildUsers = new HashSet<GuildUserModel>();
        ChannelUsers = new HashSet<ChannelUserModel>();
        ScheduledMessages = new HashSet<ScheduledMessageModel>();
    }
}