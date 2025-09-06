using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subtitles.Data;
using Subtitles.Data.Entities;
using Serilog;

namespace Subtitles.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubtitlesController : ControllerBase
{
    private readonly SubtitlesDbContext _context;

    public SubtitlesController(SubtitlesDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Subtitle>>> GetSubtitles()
    {
        Log.Information("Getting all subtitles");
        
        var subtitles = await _context.Subtitles
            .Include(s => s.Entries)
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        Log.Information("Retrieved {Count} subtitles", subtitles.Count);
        return Ok(subtitles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Subtitle>> GetSubtitle(int id)
    {
        Log.Information("Getting subtitle with ID: {SubtitleId}", id);
        
        var subtitle = await _context.Subtitles
            .Include(s => s.Entries)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (subtitle == null)
        {
            Log.Warning("Subtitle with ID {SubtitleId} not found", id);
            return NotFound();
        }

        Log.Information("Retrieved subtitle: {Title}", subtitle.Title);
        return Ok(subtitle);
    }

    [HttpPost]
    public async Task<ActionResult<Subtitle>> CreateSubtitle(CreateSubtitleRequest request)
    {
        Log.Information("Creating new subtitle: {Title}", request.Title);

        var subtitle = new Subtitle
        {
            Title = request.Title,
            Description = request.Description,
            Language = request.Language ?? "en",
            FilePath = request.FilePath,
            Format = request.Format ?? "srt"
        };

        _context.Subtitles.Add(subtitle);
        await _context.SaveChangesAsync();

        Log.Information("Created subtitle with ID: {SubtitleId}", subtitle.Id);
        return CreatedAtAction(nameof(GetSubtitle), new { id = subtitle.Id }, subtitle);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubtitle(int id, UpdateSubtitleRequest request)
    {
        Log.Information("Updating subtitle with ID: {SubtitleId}", id);

        var subtitle = await _context.Subtitles.FindAsync(id);
        if (subtitle == null || !subtitle.IsActive)
        {
            Log.Warning("Subtitle with ID {SubtitleId} not found for update", id);
            return NotFound();
        }

        subtitle.Title = request.Title ?? subtitle.Title;
        subtitle.Description = request.Description ?? subtitle.Description;
        subtitle.Language = request.Language ?? subtitle.Language;
        subtitle.FilePath = request.FilePath ?? subtitle.FilePath;
        subtitle.Format = request.Format ?? subtitle.Format;
        subtitle.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            Log.Information("Updated subtitle: {Title}", subtitle.Title);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Log.Error(ex, "Concurrency error updating subtitle with ID: {SubtitleId}", id);
            return Conflict();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubtitle(int id)
    {
        Log.Information("Deleting subtitle with ID: {SubtitleId}", id);

        var subtitle = await _context.Subtitles.FindAsync(id);
        if (subtitle == null || !subtitle.IsActive)
        {
            Log.Warning("Subtitle with ID {SubtitleId} not found for deletion", id);
            return NotFound();
        }

        // Soft delete
        subtitle.IsActive = false;
        subtitle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        Log.Information("Soft deleted subtitle: {Title}", subtitle.Title);

        return NoContent();
    }

    [HttpGet("health")]
    public async Task<ActionResult> HealthCheck()
    {
        try
        {
            // Test database connection
            await _context.Database.CanConnectAsync();
            var count = await _context.Subtitles.CountAsync();
            
            return Ok(new { 
                Status = "Healthy", 
                Database = "Connected",
                SubtitleCount = count,
                Timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Health check failed");
            return StatusCode(500, new { 
                Status = "Unhealthy", 
                Error = ex.Message,
                Timestamp = DateTime.UtcNow 
            });
        }
    }
}

public record CreateSubtitleRequest(
    string Title,
    string? Description,
    string? Language,
    string FilePath,
    string? Format
);

public record UpdateSubtitleRequest(
    string? Title,
    string? Description,
    string? Language,
    string? FilePath,
    string? Format
);
