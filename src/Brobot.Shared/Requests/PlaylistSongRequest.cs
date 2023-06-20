using System.ComponentModel.DataAnnotations;

namespace Brobot.Shared.Requests;

public record PlaylistSongRequest
{
    public int? Id { get; set; }
    
    [Required]
    [MaxLength(256)]
    public required string Name { get; set; }
    
    [Required]
    [MaxLength(1024)]
    public required string Url { get; set; }
    
    [Required]
    [MaxLength(256)]
    public required string Artist { get; set; }
    
    [Required]
    [Range(1, Int32.MaxValue)]
    public int Order { get; set; }
}