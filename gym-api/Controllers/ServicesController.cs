using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private const int MaxImageUrlLength = 10_000_000;
    private readonly AppDbContext _db;

    public ServicesController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<ServiceDto>>> GetPublic(CancellationToken cancellationToken)
    {
        var services = await _db.Services
            .Where(service => service.IsActive)
            .OrderBy(service => service.DisplayOrder)
            .ThenBy(service => service.Id)
            .ToListAsync(cancellationToken);
        return Ok(services.Select(MapToDto).ToList());
    }

    [HasPermission(PermissionCodes.ServicesView)]
    [HttpGet("all")]
    public async Task<ActionResult<List<ServiceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var services = await _db.Services
            .OrderBy(service => service.DisplayOrder)
            .ThenBy(service => service.Id)
            .ToListAsync(cancellationToken);
        return Ok(services.Select(MapToDto).ToList());
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var service = await _db.Services.FirstOrDefaultAsync(item => item.Id == id && item.IsActive, cancellationToken);
        return service is null ? NotFound(new { message = $"Service with id {id} was not found." }) : Ok(MapToDto(service));
    }

    [HasPermission(PermissionCodes.ServicesCreate)]
    [HttpPost]
    public async Task<ActionResult<ServiceDto>> Create([FromBody] CreateServiceRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        var error = Validate(request.Name, request.ShortDescription, request.ImageUrl, request.DisplayOrder);
        if (error is not null) return BadRequest(new { message = error });

        var service = new Service
        {
            Name = request.Name.Trim(),
            ShortDescription = request.ShortDescription.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            Icon = request.Icon?.Trim() ?? string.Empty,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Services.Add(service);
        await _db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, MapToDto(service));
    }

    [HasPermission(PermissionCodes.ServicesEdit)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceDto>> Update(int id, [FromBody] UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        var service = await _db.Services.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (service is null) return NotFound(new { message = $"Service with id {id} was not found." });

        var name = request.Name ?? service.Name;
        var shortDescription = request.ShortDescription ?? service.ShortDescription;
        var imageUrl = request.ImageUrl ?? service.ImageUrl;
        var displayOrder = request.DisplayOrder ?? service.DisplayOrder;
        var error = Validate(name, shortDescription, imageUrl, displayOrder);
        if (error is not null) return BadRequest(new { message = error });

        service.Name = name.Trim();
        service.ShortDescription = shortDescription.Trim();
        service.Description = request.Description?.Trim() ?? service.Description;
        service.ImageUrl = imageUrl.Trim();
        service.Icon = request.Icon?.Trim() ?? service.Icon;
        service.IsActive = request.IsActive ?? service.IsActive;
        service.DisplayOrder = displayOrder;
        service.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(MapToDto(service));
    }

    [HasPermission(PermissionCodes.ServicesDelete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var service = await _db.Services.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (service is null) return NotFound(new { message = $"Service with id {id} was not found." });
        _db.Services.Remove(service);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string? Validate(string? name, string? shortDescription, string? imageUrl, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Name is required.";
        if (string.IsNullOrWhiteSpace(shortDescription)) return "Short description is required.";
        if (displayOrder < 0) return "Display order cannot be negative.";
        if ((imageUrl?.Length ?? 0) > MaxImageUrlLength) return $"ImageUrl cannot exceed {MaxImageUrlLength:N0} characters.";
        return null;
    }

    private static ServiceDto MapToDto(Service service) => new()
    {
        Id = service.Id,
        Name = service.Name,
        ShortDescription = service.ShortDescription,
        Description = service.Description,
        ImageUrl = service.ImageUrl,
        Icon = service.Icon,
        IsActive = service.IsActive,
        DisplayOrder = service.DisplayOrder,
        CreatedAt = service.CreatedAt,
        UpdatedAt = service.UpdatedAt
    };
}
