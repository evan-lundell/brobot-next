namespace Brobot.Models;

public class GuildModel
{
    public ulong Id { get; set; }
    public required string Name { get; set; }
    public bool Archived { get; set; }

    public ICollection<ChannelModel> Channels { get; set; }
    public ICollection<GuildUserModel> GuildUsers { get; set; }

    public GuildModel()
    {
        Channels = new HashSet<ChannelModel>();
        GuildUsers = new HashSet<GuildUserModel>();
    }
}