using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController]
[Route("api/about")]
public class AboutController : ControllerBase
{
    private const int MaxImageUrlLength = 10_000_000;
    private readonly AppDbContext _db;

    public AboutController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HasPermission(PermissionCodes.AboutView)]
    [HttpGet]
    public async Task<ActionResult<AboutDto>> GetAbouts(CancellationToken cancellationToken)
    {
        var about = await _db.Abouts
            .OrderByDescending(a => a.UpdatedAt)
            .ThenByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return about is null ? NotFound(new { message = "No about content found." }) : Ok(MapToDto(about));
    }

    [HasPermission(PermissionCodes.AboutView)]
    [HttpGet("all")]
    public async Task<ActionResult<List<AboutDto>>> GetAll(CancellationToken cancellationToken)
    {
        var abouts = await _db.Abouts
            .OrderByDescending(a => a.UpdatedAt)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
        return Ok(abouts.Select(MapToDto).ToList());
    }

    [AllowAnonymous]
    [HasPermission(PermissionCodes.AboutView)]
    [HttpGet("active")]
    public async Task<ActionResult<AboutDto>> GetActiveAbout(CancellationToken cancellationToken)
    {
        var about = await _db.Abouts
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.UpdatedAt)
            .ThenByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return about is null ? NotFound(new { message = "No active about content found." }) : Ok(MapToDto(about));
    }

    [HasPermission(PermissionCodes.AboutCreate)]
    [HttpPost]
    public async Task<ActionResult<AboutDto>> Create([FromBody] CreateAboutRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        if (await _db.Abouts.AnyAsync(cancellationToken))
        {
            return Conflict(new { message = "About content already exists. Update the existing record instead." });
        }
        var error = Validate(request.Title, request.Description, request.ImageUrl, request.ExperienceYears, request.MembersCount, request.TrainersCount);
        if (error is not null) return BadRequest(new { message = error });

        var about = new About
        {
            Title = request.Title.Trim(),
            Subtitle = request.Subtitle?.Trim() ?? string.Empty,
            Description = request.Description.Trim(),
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            ExperienceYears = request.ExperienceYears,
            MembersCount = request.MembersCount,
            TrainersCount = request.TrainersCount,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Abouts.Add(about);
        await _db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetActiveAbout), new { id = about.Id }, MapToDto(about));
    }

    [HasPermission(PermissionCodes.AboutEdit)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AboutDto>> Update(int id, [FromBody] UpdateAboutRequest request, CancellationToken cancellationToken)
    {
        var about = await _db.Abouts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (about is null) return NotFound(new { message = $"About with id {id} was not found." });

        var title = request.Title ?? about.Title;
        var description = request.Description ?? about.Description;
        var experienceYears = request.ExperienceYears ?? about.ExperienceYears;
        var membersCount = request.MembersCount ?? about.MembersCount;
        var trainersCount = request.TrainersCount ?? about.TrainersCount;
        var imageUrl = request.ImageUrl ?? about.ImageUrl;
        var error = Validate(title, description, imageUrl, experienceYears, membersCount, trainersCount);
        if (error is not null) return BadRequest(new { message = error });

        about.Title = title.Trim();
        about.Subtitle = request.Subtitle?.Trim() ?? about.Subtitle;
        about.Description = description.Trim();
        about.ImageUrl = imageUrl.Trim();
        about.ExperienceYears = experienceYears;
        about.MembersCount = membersCount;
        about.TrainersCount = trainersCount;
        about.IsActive = request.IsActive ?? about.IsActive;
        about.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(MapToDto(about));
    }

    [HasPermission(PermissionCodes.AboutDelete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var about = await _db.Abouts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (about is null) return NotFound(new { message = $"About with id {id} was not found." });
        _db.Abouts.Remove(about);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string? Validate(string? title, string? description, string? imageUrl, int? experienceYears, int? membersCount, int? trainersCount)
    {
        if (string.IsNullOrWhiteSpace(title)) return "Title is required.";
        if (string.IsNullOrWhiteSpace(description)) return "Description is required.";
        if ((imageUrl?.Length ?? 0) > MaxImageUrlLength) return $"ImageUrl cannot exceed {MaxImageUrlLength:N0} characters.";
        if (experienceYears < 0 || membersCount < 0 || trainersCount < 0) return "Numeric values cannot be negative.";
        return null;
    }

    private static AboutDto MapToDto(About about) => new()
    {
        Id = about.Id, Title = about.Title, Subtitle = about.Subtitle, Description = about.Description,
        ImageUrl = about.ImageUrl, ExperienceYears = about.ExperienceYears, MembersCount = about.MembersCount,
        TrainersCount = about.TrainersCount, IsActive = about.IsActive, CreatedAt = about.CreatedAt, UpdatedAt = about.UpdatedAt
    };
}
