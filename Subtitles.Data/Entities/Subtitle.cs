using System.ComponentModel.DataAnnotations;

namespace Subtitles.Data.Entities;

public class Subtitle
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Language { get; set; } = "en";
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Format { get; set; } = "srt";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<SubtitleEntry> Entries { get; set; } = new List<SubtitleEntry>();
}

