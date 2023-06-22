namespace Brobot.Models;

public class PlaylistSongModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string Artist { get; set; }
    public required PlaylistModel Playlist { get; set; }
    public int Order { get; set; }
    public int PlaylistId { get; set; }
}