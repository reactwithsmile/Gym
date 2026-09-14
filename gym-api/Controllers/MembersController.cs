using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymApi.Controllers;

[ApiController, Route("api/members")]
public class MembersController(AppDbContext db) : ControllerBase
{
    [HasPermission(PermissionCodes.MembersView)]
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var members = await db.Members
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Email,
                x.Phone,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt,
                CreatedBy = x.CreatedByUser == null ? null : new { x.CreatedByUser.Name, Role = x.CreatedByUser.Role.Name },
                UpdatedBy = x.UpdatedByUser == null ? null : new { x.UpdatedByUser.Name, Role = x.UpdatedByUser.Role.Name },
                Memberships = x.Memberships.Select(m => new
                {
                    m.Id,
                    m.MemberId,
                    m.MembershipPlanId,
                    m.StartDate,
                    m.EndDate,
                    m.Status
                }).ToList()
            })
            .ToListAsync(ct);

        return Ok(members);
    }

    [HasPermission(PermissionCodes.MembersView)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var member = await db.Members
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Email,
                x.Phone,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt,
                CreatedBy = x.CreatedByUser == null ? null : new { x.CreatedByUser.Name, Role = x.CreatedByUser.Role.Name },
                UpdatedBy = x.UpdatedByUser == null ? null : new { x.UpdatedByUser.Name, Role = x.UpdatedByUser.Role.Name },
                Memberships = x.Memberships.Select(m => new
                {
                    m.Id,
                    m.MemberId,
                    m.MembershipPlanId,
                    m.StartDate,
                    m.EndDate,
                    m.Status,
                    MembershipPlan = m.MembershipPlan == null ? null : new
                    {
                        m.MembershipPlan.Id,
                        m.MembershipPlan.Name
                    }
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);
        return member is null ? NotFound() : Ok(member);
    }

    [HasPermission(PermissionCodes.MembersCreate)]
    [HttpPost]
    public async Task<IActionResult> Create(MemberRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Name and email are required." });
        var userId = CurrentUserId();
        var member = new Member { Name = request.Name.Trim(), Email = request.Email.Trim(), Phone = request.Phone?.Trim() ?? "", IsActive = request.IsActive, CreatedByUserId = userId, UpdatedByUserId = userId };
        db.Members.Add(member); await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = member.Id }, member);
    }

    [HasPermission(PermissionCodes.MembersEdit)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, MemberRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null) return NotFound();
        member.Name = request.Name.Trim(); member.Email = request.Email.Trim(); member.Phone = request.Phone?.Trim() ?? ""; member.IsActive = request.IsActive; member.UpdatedAt = DateTime.UtcNow;
        member.UpdatedByUserId = CurrentUserId();
        await db.SaveChangesAsync(ct); return Ok(member);
    }

    [HasPermission(PermissionCodes.MembersCreate)]
    [HttpPost("memberships")]
    public async Task<IActionResult> AddMembership(MembershipRequest request, CancellationToken ct)
    {
        if (!await db.Members.AnyAsync(x => x.Id == request.MemberId, ct))
            return BadRequest(new { message = "Member was not found." });
        var today = DateTime.UtcNow.Date;
        if (request.StartDate.Date < today || request.EndDate.Date <= request.StartDate.Date)
            return BadRequest(new { message = "Membership start date cannot be in the past and end date must be after start date." });
        if (request.MembershipPlanId.HasValue && !await db.MembershipPlans.AnyAsync(x => x.Id == request.MembershipPlanId, ct))
            return BadRequest(new { message = "Membership plan was not found." });
        var membership = new Membership { MemberId = request.MemberId, MembershipPlanId = request.MembershipPlanId, StartDate = request.StartDate, EndDate = request.EndDate, Status = "Active" };
        db.Memberships.Add(membership); await db.SaveChangesAsync(ct);
        return Ok(membership);
    }

    private int? CurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
}
