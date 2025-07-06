using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brobot.Models;

[Table("word_count")]
public class WordCountModel
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("word")]
    [MaxLength(255)]
    public required string Word { get; set; }
    
    [Column("count")]
    public int Count { get; set; }
    
    [Column("channel_id")]
    public ulong ChannelId { get; set; }
    public virtual required ChannelModel Channel { get; set; }
    
    [Column("count_date")]
    public DateOnly CountDate { get; set; }
}