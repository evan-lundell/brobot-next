using System.ComponentModel.DataAnnotations;

namespace Brobot.Shared.Requests;

public record ScheduledMessageRequest
{
    public int? Id { get; set; }
    
    [Required]
    [MaxLength(250)]
    public required string MessageText { get; set; }
    
    [Required]
    public DateTime SendDate { get; set; }
    
    [Required]
    public ulong? ChannelId { get; set; }
}

