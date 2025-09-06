using System.ComponentModel.DataAnnotations;

namespace Subtitles.Data.Entities;

public class SubtitleEntry
{
    public int Id { get; set; }
    
    public int SubtitleId { get; set; }
    
    public int SequenceNumber { get; set; }
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Subtitle Subtitle { get; set; } = null!;
}
