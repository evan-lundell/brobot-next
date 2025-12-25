// ReSharper disable VirtualMemberCallInConstructor
namespace Brobot.Models;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class GuildModel
{
    public ulong Id { get; init; }
    public required string Name { get; set; }
    public bool Archived { get; set; }
    
    public ulong? PrimaryChannelId { get; init; }
    public ChannelModel? PrimaryChannel { get; init; }
    
    public virtual ICollection<ChannelModel> Channels { get; init; }
    public virtual ICollection<GuildDiscordUserModel> GuildDiscordUsers { get; init; }

    public GuildModel()
    {
        Channels = new HashSet<ChannelModel>();
        GuildDiscordUsers = new HashSet<GuildDiscordUserModel>();
    }
}