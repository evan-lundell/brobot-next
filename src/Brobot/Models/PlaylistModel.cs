namespace Brobot.Models;

public class PlaylistModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required UserModel User { get; set; }
    public ulong UserId { get; set; }
    public virtual ICollection<PlaylistSongModel> Songs { get; set; }

    public PlaylistModel()
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Songs = new HashSet<PlaylistSongModel>();
    }
}