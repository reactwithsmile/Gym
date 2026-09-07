using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController]
[Route("api/gym-settings")]
public class GymSettingsController : ControllerBase
{
    private readonly AppDbContext _db;
    public GymSettingsController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<GymSettingsDto>> Get(CancellationToken cancellationToken)
    {
        var settings = await _db.GymSettings.OrderByDescending(item => item.UpdatedAt).ThenByDescending(item => item.Id).FirstOrDefaultAsync(cancellationToken);
        return settings is null ? NotFound(new { message = "No gym settings found." }) : Ok(MapToDto(settings));
    }

    [HasPermission(PermissionCodes.SettingsEdit)]
    [HttpPost]
    public async Task<ActionResult<GymSettingsDto>> Create([FromBody] CreateGymSettingsRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        if (await _db.GymSettings.AnyAsync(cancellationToken)) return Conflict(new { message = "Gym settings already exist. Update the existing record instead." });
        var error = Validate(request);
        if (error is not null) return BadRequest(new { message = error });
        var settings = MapToEntity(request);
        _db.GymSettings.Add(settings);
        await _db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = settings.Id }, MapToDto(settings));
    }

    [HasPermission(PermissionCodes.SettingsEdit)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<GymSettingsDto>> Update(int id, [FromBody] UpdateGymSettingsRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        var settings = await _db.GymSettings.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (settings is null) return NotFound(new { message = $"Gym settings with id {id} were not found." });
        var error = Validate(request);
        if (error is not null) return BadRequest(new { message = error });
        settings.GymName = request.GymName.Trim();
        settings.LogoUrl = request.LogoUrl?.Trim() ?? string.Empty;
        settings.Tagline = request.Tagline?.Trim() ?? string.Empty;
        settings.WebsiteUrl = request.WebsiteUrl?.Trim() ?? string.Empty;
        settings.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "INR" : request.Currency.Trim();
        settings.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(MapToDto(settings));
    }

    private static string? Validate(CreateGymSettingsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.GymName)) return "Gym name is required.";
        return null;
    }

    private static GymSettings MapToEntity(CreateGymSettingsRequest request) => new()
    {
        GymName = request.GymName.Trim(), LogoUrl = request.LogoUrl?.Trim() ?? string.Empty,
        Tagline = request.Tagline?.Trim() ?? string.Empty,
        WebsiteUrl = request.WebsiteUrl?.Trim() ?? string.Empty,
        Currency = string.IsNullOrWhiteSpace(request.Currency) ? "INR" : request.Currency.Trim(),
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    private static GymSettingsDto MapToDto(GymSettings settings) => new()
    {
        Id = settings.Id, GymName = settings.GymName, LogoUrl = settings.LogoUrl,
        Tagline = settings.Tagline, WebsiteUrl = settings.WebsiteUrl, Currency = settings.Currency,
        CreatedAt = settings.CreatedAt, UpdatedAt = settings.UpdatedAt
    };
}
