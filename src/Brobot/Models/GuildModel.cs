// ReSharper disable VirtualMemberCallInConstructor
namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class GuildModel
{
    public ulong Id { get; set; }
    public required string Name { get; set; }
    public bool Archived { get; set; }
    
    public ulong? PrimaryChannelId { get; set; }
    public ChannelModel? PrimaryChannel { get; set; }
    
    public virtual ICollection<ChannelModel> Channels { get; set; }
    public virtual ICollection<GuildDiscordUserModel> GuildUsers { get; set; }

    public GuildModel()
    {
        Channels = new HashSet<ChannelModel>();
        GuildUsers = new HashSet<GuildDiscordUserModel>();
    }
}