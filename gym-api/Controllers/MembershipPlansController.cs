using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController]
[Route("api/membership-plans")]
public class MembershipPlansController : ControllerBase
{
    private readonly AppDbContext _db;
    public MembershipPlansController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<MembershipPlanDto>>> GetPublic(CancellationToken cancellationToken)
    {
        var plans = await _db.MembershipPlans.Where(plan => plan.IsActive)
            .OrderBy(plan => plan.DisplayOrder).ThenBy(plan => plan.Id).ToListAsync(cancellationToken);
        return Ok(plans.Select(MapToDto).ToList());
    }

    [HasPermission(PermissionCodes.MembershipView)]
    [HttpGet("all")]
    public async Task<ActionResult<List<MembershipPlanDto>>> GetAll(CancellationToken cancellationToken)
    {
        var plans = await _db.MembershipPlans.OrderBy(plan => plan.DisplayOrder).ThenBy(plan => plan.Id).ToListAsync(cancellationToken);
        return Ok(plans.Select(MapToDto).ToList());
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MembershipPlanDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var plan = await _db.MembershipPlans.FirstOrDefaultAsync(item => item.Id == id && item.IsActive, cancellationToken);
        return plan is null ? NotFound(new { message = $"Membership plan with id {id} was not found." }) : Ok(MapToDto(plan));
    }

    [HasPermission(PermissionCodes.MembershipCreate)]
    [HttpPost]
    public async Task<ActionResult<MembershipPlanDto>> Create([FromBody] CreateMembershipPlanRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        var error = Validate(request.Name, request.ShortDescription, request.Price, request.DurationMonths, request.DisplayOrder);
        if (error is not null) return BadRequest(new { message = error });
        var plan = new MembershipPlan
        {
            Name = request.Name.Trim(), ShortDescription = request.ShortDescription.Trim(),
            Description = request.Description?.Trim() ?? string.Empty, Price = request.Price,
            DurationMonths = request.DurationMonths, Features = request.Features?.Trim() ?? string.Empty,
            DisplayOrder = request.DisplayOrder, IsPopular = request.IsPopular, IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _db.MembershipPlans.Add(plan);
        await _db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, MapToDto(plan));
    }

    [HasPermission(PermissionCodes.MembershipEdit)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MembershipPlanDto>> Update(int id, [FromBody] UpdateMembershipPlanRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        var plan = await _db.MembershipPlans.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (plan is null) return NotFound(new { message = $"Membership plan with id {id} was not found." });
        var name = request.Name ?? plan.Name;
        var shortDescription = request.ShortDescription ?? plan.ShortDescription;
        var price = request.Price ?? plan.Price;
        var duration = request.DurationMonths ?? plan.DurationMonths;
        var order = request.DisplayOrder ?? plan.DisplayOrder;
        var error = Validate(name, shortDescription, price, duration, order);
        if (error is not null) return BadRequest(new { message = error });
        plan.Name = name.Trim(); plan.ShortDescription = shortDescription.Trim();
        plan.Description = request.Description?.Trim() ?? plan.Description;
        plan.Price = price; plan.DurationMonths = duration;
        plan.Features = request.Features?.Trim() ?? plan.Features;
        plan.DisplayOrder = order; plan.IsPopular = request.IsPopular ?? plan.IsPopular;
        plan.IsActive = request.IsActive ?? plan.IsActive; plan.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(MapToDto(plan));
    }

    [HasPermission(PermissionCodes.MembershipDelete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var plan = await _db.MembershipPlans.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (plan is null) return NotFound(new { message = $"Membership plan with id {id} was not found." });
        _db.MembershipPlans.Remove(plan);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string? Validate(string? name, string? shortDescription, decimal price, int duration, int order)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Name is required.";
        if (string.IsNullOrWhiteSpace(shortDescription)) return "Short description is required.";
        if (price < 0) return "Price cannot be negative.";
        if (duration <= 0) return "Duration must be greater than 0.";
        if (order < 0) return "Display order cannot be negative.";
        return null;
    }

    private static MembershipPlanDto MapToDto(MembershipPlan plan) => new()
    {
        Id = plan.Id, Name = plan.Name, ShortDescription = plan.ShortDescription,
        Description = plan.Description, Price = plan.Price, DurationMonths = plan.DurationMonths,
        Features = plan.Features, DisplayOrder = plan.DisplayOrder, IsPopular = plan.IsPopular,
        IsActive = plan.IsActive, CreatedAt = plan.CreatedAt, UpdatedAt = plan.UpdatedAt
    };
}
