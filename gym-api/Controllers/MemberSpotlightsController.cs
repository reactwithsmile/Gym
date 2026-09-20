using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController, Route("api/member-spotlights")]
public class MemberSpotlightsController(AppDbContext db) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetFeatured(CancellationToken ct)
    {
        var item = (await db.MemberSpotlights
            .Where(x => x.IsActive && x.IsFeatured)
            .OrderBy(x => x.DisplayOrder).ThenByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(ct));
        return Ok(item is null ? null : Map(item));
    }

    [HasPermission(PermissionCodes.MemberSpotlightView)]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok((await db.MemberSpotlights.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).ToListAsync(ct)).Select(Map).ToList());

    [HasPermission(PermissionCodes.MemberSpotlightCreate)]
    [HttpPost]
    public async Task<IActionResult> Create(MemberSpotlightDto request, CancellationToken ct)
    {
        var error = Validate(request);
        if (error is not null) return BadRequest(new { message = error });
        var item = From(request);
        db.MemberSpotlights.Add(item);
        await db.SaveChangesAsync(ct);
        return Ok(item);
    }

    [HasPermission(PermissionCodes.MemberSpotlightEdit)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, MemberSpotlightDto request, CancellationToken ct)
    {
        var item = await db.MemberSpotlights.FindAsync([id], ct);
        if (item is null) return NotFound();
        var error = Validate(request);
        if (error is not null) return BadRequest(new { message = error });
        item.MemberName = request.MemberName.Trim(); item.Quote = request.Quote?.Trim() ?? "";
        item.Story = request.Story?.Trim() ?? ""; item.ImageUrl = request.ImageUrl?.Trim() ?? "";
        item.DurationText = request.DurationText?.Trim() ?? ""; item.AchievementText = request.AchievementText?.Trim() ?? "";
        item.DisplayOrder = request.DisplayOrder; item.IsActive = request.IsActive; item.IsFeatured = request.IsFeatured;
        item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(item);
    }

    [HasPermission(PermissionCodes.MemberSpotlightDelete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await db.MemberSpotlights.FindAsync([id], ct);
        if (item is null) return NotFound();
        db.MemberSpotlights.Remove(item);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static string? Validate(MemberSpotlightDto item) =>
        string.IsNullOrWhiteSpace(item.MemberName) ? "Member name is required." :
        item.DisplayOrder < 0 ? "Display order cannot be negative." : null;

    private static MemberSpotlightDto Map(MemberSpotlight item) => new()
    {
        Id = item.Id, MemberName = item.MemberName, Quote = item.Quote, Story = item.Story, ImageUrl = item.ImageUrl,
        DurationText = item.DurationText, AchievementText = item.AchievementText, DisplayOrder = item.DisplayOrder,
        IsActive = item.IsActive, IsFeatured = item.IsFeatured, CreatedAt = item.CreatedAt, UpdatedAt = item.UpdatedAt
    };
    private static MemberSpotlight From(MemberSpotlightDto item) => new()
    {
        MemberName = item.MemberName.Trim(), Quote = item.Quote?.Trim() ?? "", Story = item.Story?.Trim() ?? "",
        ImageUrl = item.ImageUrl?.Trim() ?? "", DurationText = item.DurationText?.Trim() ?? "",
        AchievementText = item.AchievementText?.Trim() ?? "", DisplayOrder = item.DisplayOrder,
        IsActive = item.IsActive, IsFeatured = item.IsFeatured
    };
}
