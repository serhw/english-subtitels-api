using Microsoft.EntityFrameworkCore;
using Subtitles.Data.Entities;

namespace Subtitles.Data;

public class SubtitlesDbContext : DbContext
{
    public SubtitlesDbContext(DbContextOptions<SubtitlesDbContext> options) : base(options)
    {
    }

    public DbSet<Subtitle> Subtitles { get; set; }
    public DbSet<SubtitleEntry> SubtitleEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Subtitle entity
        modelBuilder.Entity<Subtitle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Language).IsRequired().HasMaxLength(10).HasDefaultValue("en");
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Format).HasMaxLength(50).HasDefaultValue("srt");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasIndex(e => e.Language);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.Title, e.Language });
        });

        // Configure SubtitleEntry entity
        modelBuilder.Entity<SubtitleEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasIndex(e => e.SubtitleId);
            entity.HasIndex(e => e.SequenceNumber);
            entity.HasIndex(e => new { e.SubtitleId, e.SequenceNumber });

            // Configure relationship
            entity.HasOne(e => e.Subtitle)
                  .WithMany(s => s.Entries)
                  .HasForeignKey(e => e.SubtitleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
