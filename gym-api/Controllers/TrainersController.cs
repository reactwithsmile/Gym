using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController]
[Route("api/trainers")]
public class TrainersController : ControllerBase
{
    private const int MaxImageUrlLength = 10_000_000;
    private readonly AppDbContext _db;

    public TrainersController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<TrainerDto>>> GetPublic(CancellationToken cancellationToken)
    {
        var trainers = await _db.Trainers
            .Where(trainer => trainer.IsActive)
            .OrderBy(trainer => trainer.DisplayOrder)
            .ThenBy(trainer => trainer.Id)
            .ToListAsync(cancellationToken);
        return Ok(trainers.Select(MapToDto).ToList());
    }

    [HasPermission(PermissionCodes.TrainersView)]
    [HttpGet("all")]
    public async Task<ActionResult<List<TrainerDto>>> GetAll(CancellationToken cancellationToken)
    {
        var trainers = await _db.Trainers
            .OrderBy(trainer => trainer.DisplayOrder)
            .ThenBy(trainer => trainer.Id)
            .ToListAsync(cancellationToken);
        return Ok(trainers.Select(MapToDto).ToList());
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TrainerDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var trainer = await _db.Trainers.FirstOrDefaultAsync(item => item.Id == id && item.IsActive, cancellationToken);
        return trainer is null ? NotFound(new { message = $"Trainer with id {id} was not found." }) : Ok(MapToDto(trainer));
    }

    [HasPermission(PermissionCodes.TrainersCreate)]
    [HttpPost]
    public async Task<ActionResult<TrainerDto>> Create([FromBody] CreateTrainerRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        var error = Validate(request.Name, request.Role, request.ImageUrl, request.ExperienceYears, request.DisplayOrder);
        if (error is not null) return BadRequest(new { message = error });

        var trainer = new Trainer
        {
            Name = request.Name.Trim(),
            Role = request.Role.Trim(),
            Bio = request.Bio?.Trim() ?? string.Empty,
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            Specialization = request.Specialization?.Trim() ?? string.Empty,
            ExperienceYears = request.ExperienceYears,
            InstagramUrl = request.InstagramUrl?.Trim() ?? string.Empty,
            FacebookUrl = request.FacebookUrl?.Trim() ?? string.Empty,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Trainers.Add(trainer);
        await _db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = trainer.Id }, MapToDto(trainer));
    }

    [HasPermission(PermissionCodes.TrainersEdit)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TrainerDto>> Update(int id, [FromBody] UpdateTrainerRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        var trainer = await _db.Trainers.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (trainer is null) return NotFound(new { message = $"Trainer with id {id} was not found." });

        var name = request.Name ?? trainer.Name;
        var role = request.Role ?? trainer.Role;
        var imageUrl = request.ImageUrl ?? trainer.ImageUrl;
        var experienceYears = request.ExperienceYears ?? trainer.ExperienceYears;
        var displayOrder = request.DisplayOrder ?? trainer.DisplayOrder;
        var error = Validate(name, role, imageUrl, experienceYears, displayOrder);
        if (error is not null) return BadRequest(new { message = error });

        trainer.Name = name.Trim();
        trainer.Role = role.Trim();
        trainer.Bio = request.Bio?.Trim() ?? trainer.Bio;
        trainer.ImageUrl = imageUrl.Trim();
        trainer.Specialization = request.Specialization?.Trim() ?? trainer.Specialization;
        trainer.ExperienceYears = experienceYears;
        trainer.InstagramUrl = request.InstagramUrl?.Trim() ?? trainer.InstagramUrl;
        trainer.FacebookUrl = request.FacebookUrl?.Trim() ?? trainer.FacebookUrl;
        trainer.DisplayOrder = displayOrder;
        trainer.IsActive = request.IsActive ?? trainer.IsActive;
        trainer.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(MapToDto(trainer));
    }

    [HasPermission(PermissionCodes.TrainersDelete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var trainer = await _db.Trainers.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (trainer is null) return NotFound(new { message = $"Trainer with id {id} was not found." });
        _db.Trainers.Remove(trainer);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string? Validate(string? name, string? role, string? imageUrl, int experienceYears, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Name is required.";
        if (string.IsNullOrWhiteSpace(role)) return "Role is required.";
        if (experienceYears < 0) return "Experience years cannot be negative.";
        if (displayOrder < 0) return "Display order cannot be negative.";
        if ((imageUrl?.Length ?? 0) > MaxImageUrlLength) return $"ImageUrl cannot exceed {MaxImageUrlLength:N0} characters.";
        return null;
    }

    private static TrainerDto MapToDto(Trainer trainer) => new()
    {
        Id = trainer.Id,
        Name = trainer.Name,
        Role = trainer.Role,
        Bio = trainer.Bio,
        ImageUrl = trainer.ImageUrl,
        Specialization = trainer.Specialization,
        ExperienceYears = trainer.ExperienceYears,
        InstagramUrl = trainer.InstagramUrl,
        FacebookUrl = trainer.FacebookUrl,
        DisplayOrder = trainer.DisplayOrder,
        IsActive = trainer.IsActive,
        CreatedAt = trainer.CreatedAt,
        UpdatedAt = trainer.UpdatedAt
    };
}
