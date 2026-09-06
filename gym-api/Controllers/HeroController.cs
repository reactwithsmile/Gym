using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController]
[Route("api/hero")]
public class HeroController : ControllerBase
{
    private readonly AppDbContext _db;

    public HeroController(AppDbContext db)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HasPermission(PermissionCodes.HeroView)]
    [HttpGet]
    public async Task<ActionResult<HeroDto>> GetActiveHero(CancellationToken cancellationToken)
    {
        var hero = await _db.Heroes
            .Where(h => h.IsActive)
            .OrderByDescending(h => h.UpdatedAt)
            .ThenByDescending(h => h.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (hero is null)
        {
            return NotFound(new { message = "No active hero content found." });
        }

        return Ok(MapToDto(hero));
    }

    [HasPermission(PermissionCodes.HeroCreate)]
    [HttpPost]
    public async Task<ActionResult<HeroDto>> CreateHero([FromBody] CreateHeroRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        var validationError = ValidateHeroRequest(request.Heading, request.Description, request.ImageUrl,
            request.PrimaryButtonText, request.PrimaryButtonLink,
            request.SecondaryButtonText, request.SecondaryButtonLink);

        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var hero = new Hero
        {
            Heading = request.Heading.Trim(),
            Description = request.Description.Trim(),
            PrimaryButtonText = request.PrimaryButtonText.Trim(),
            PrimaryButtonLink = request.PrimaryButtonLink.Trim(),
            SecondaryButtonText = request.SecondaryButtonText.Trim(),
            SecondaryButtonLink = request.SecondaryButtonLink.Trim(),
            ImageUrl = request.ImageUrl.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Heroes.Add(hero);
        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetActiveHero), new { id = hero.Id }, MapToDto(hero));
    }

    [HasPermission(PermissionCodes.HeroEdit)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<HeroDto>> UpdateHero(int id, [FromBody] UpdateHeroRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        var hero = await _db.Heroes.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
        if (hero is null)
        {
            return NotFound(new { message = $"Hero with id {id} was not found." });
        }

        var heading = request.Heading ?? hero.Heading;
        var description = request.Description ?? hero.Description;
        var imageUrl = request.ImageUrl ?? hero.ImageUrl;
        var primaryButtonText = request.PrimaryButtonText ?? hero.PrimaryButtonText;
        var primaryButtonLink = request.PrimaryButtonLink ?? hero.PrimaryButtonLink;
        var secondaryButtonText = request.SecondaryButtonText ?? hero.SecondaryButtonText;
        var secondaryButtonLink = request.SecondaryButtonLink ?? hero.SecondaryButtonLink;
        var isActive = request.IsActive ?? hero.IsActive;

        var validationError = ValidateHeroRequest(heading, description, imageUrl,
            primaryButtonText, primaryButtonLink,
            secondaryButtonText, secondaryButtonLink);

        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        hero.Heading = heading.Trim();
        hero.Description = description.Trim();
        hero.ImageUrl = imageUrl.Trim();
        hero.PrimaryButtonText = primaryButtonText.Trim();
        hero.PrimaryButtonLink = primaryButtonLink.Trim();
        hero.SecondaryButtonText = secondaryButtonText.Trim();
        hero.SecondaryButtonLink = secondaryButtonLink.Trim();
        hero.IsActive = isActive;
        hero.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(MapToDto(hero));
    }

    [HasPermission(PermissionCodes.HeroDelete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHero(int id, CancellationToken cancellationToken)
    {
        var hero = await _db.Heroes.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
        if (hero is null)
        {
            return NotFound(new { message = $"Hero with id {id} was not found." });
        }

        _db.Heroes.Remove(hero);
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static string? ValidateHeroRequest(
        string heading,
        string description,
        string imageUrl,
        string primaryButtonText,
        string primaryButtonLink,
        string secondaryButtonText,
        string secondaryButtonLink)
    {
        if (string.IsNullOrWhiteSpace(heading))
        {
            return "Heading is required.";
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return "Description is required.";
        }

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return "ImageUrl is required.";
        }

        if (HasButtonText(primaryButtonText) != HasButtonLink(primaryButtonLink))
        {
            return "Primary button text and link must be provided together.";
        }

        if (HasButtonText(secondaryButtonText) != HasButtonLink(secondaryButtonLink))
        {
            return "Secondary button text and link must be provided together.";
        }

        return null;
    }

    private static bool HasButtonText(string value) => !string.IsNullOrWhiteSpace(value);
    private static bool HasButtonLink(string value) => !string.IsNullOrWhiteSpace(value);

    private static HeroDto MapToDto(Hero hero)
    {
        return new HeroDto
        {
            Id = hero.Id,
            Heading = hero.Heading,
            Description = hero.Description,
            PrimaryButtonText = hero.PrimaryButtonText,
            PrimaryButtonLink = hero.PrimaryButtonLink,
            SecondaryButtonText = hero.SecondaryButtonText,
            SecondaryButtonLink = hero.SecondaryButtonLink,
            ImageUrl = hero.ImageUrl,
            IsActive = hero.IsActive,
            CreatedAt = hero.CreatedAt,
            UpdatedAt = hero.UpdatedAt
        };
    }
}
