using System.ComponentModel.DataAnnotations;
using Brobot.Shared.Responses;

namespace Brobot.Shared.Requests;

public record PlaylistRequest
{
    public int? Id { get; set; }
    
    [Required]
    [MaxLength(256)]
    public required string Name { get; set; }

    public IEnumerable<PlaylistSongResponse> Songs { get; set; } = new List<PlaylistSongResponse>();
}