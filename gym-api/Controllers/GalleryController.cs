using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController]
[Route("api/gallery")]
public class GalleryController : ControllerBase
{
    private const int MaxImageUrlLength = 10_000_000;
    private readonly AppDbContext _db;

    public GalleryController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<GalleryDto>>> GetPublic(CancellationToken cancellationToken)
    {
        var items = await _db.Gallery
            .Where(item => item.IsActive)
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
        return Ok(items.Select(MapToDto).ToList());
    }

    [HasPermission(PermissionCodes.GalleryView)]
    [HttpGet("all")]
    public async Task<ActionResult<List<GalleryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _db.Gallery
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
        return Ok(items.Select(MapToDto).ToList());
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GalleryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _db.Gallery.FirstOrDefaultAsync(gallery => gallery.Id == id && gallery.IsActive, cancellationToken);
        return item is null ? NotFound(new { message = $"Gallery item with id {id} was not found." }) : Ok(MapToDto(item));
    }

    [HasPermission(PermissionCodes.GalleryCreate)]
    [HttpPost]
    public async Task<ActionResult<GalleryDto>> Create([FromBody] CreateGalleryRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        var error = Validate(request.Title, request.ImageUrl, request.DisplayOrder);
        if (error is not null) return BadRequest(new { message = error });

        var item = new Gallery
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            ImageUrl = request.ImageUrl.Trim(),
            Category = request.Category?.Trim() ?? string.Empty,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Gallery.Add(item);
        await _db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToDto(item));
    }

    [HasPermission(PermissionCodes.GalleryEdit)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<GalleryDto>> Update(int id, [FromBody] UpdateGalleryRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        var item = await _db.Gallery.FirstOrDefaultAsync(gallery => gallery.Id == id, cancellationToken);
        if (item is null) return NotFound(new { message = $"Gallery item with id {id} was not found." });

        var title = request.Title ?? item.Title;
        var imageUrl = request.ImageUrl ?? item.ImageUrl;
        var displayOrder = request.DisplayOrder ?? item.DisplayOrder;
        var error = Validate(title, imageUrl, displayOrder);
        if (error is not null) return BadRequest(new { message = error });

        item.Title = title.Trim();
        item.Description = request.Description?.Trim() ?? item.Description;
        item.ImageUrl = imageUrl.Trim();
        item.Category = request.Category?.Trim() ?? item.Category;
        item.DisplayOrder = displayOrder;
        item.IsActive = request.IsActive ?? item.IsActive;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(MapToDto(item));
    }

    [HasPermission(PermissionCodes.GalleryDelete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var item = await _db.Gallery.FirstOrDefaultAsync(gallery => gallery.Id == id, cancellationToken);
        if (item is null) return NotFound(new { message = $"Gallery item with id {id} was not found." });
        _db.Gallery.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string? Validate(string? title, string? imageUrl, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(title)) return "Title is required.";
        if (string.IsNullOrWhiteSpace(imageUrl)) return "Image URL is required.";
        if (displayOrder < 0) return "Display order cannot be negative.";
        if (imageUrl.Length > MaxImageUrlLength) return $"ImageUrl cannot exceed {MaxImageUrlLength:N0} characters.";
        return null;
    }

    private static GalleryDto MapToDto(Gallery item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        ImageUrl = item.ImageUrl,
        Category = item.Category,
        DisplayOrder = item.DisplayOrder,
        IsActive = item.IsActive,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt
    };
}
