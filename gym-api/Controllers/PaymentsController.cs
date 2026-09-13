using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using GymApi.Hubs;

namespace GymApi.Controllers;

[ApiController, Route("api/payments")]
public class PaymentsController(AppDbContext db, IHubContext<NotificationHub> hub) : ControllerBase
{
    [HasPermission(PermissionCodes.FeesView)]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? memberId, CancellationToken ct)
    {
        var payments = await db.Payments
            .Where(x => !memberId.HasValue || x.MemberId == memberId.Value)
            .OrderByDescending(x => x.PaidAt)
            .Select(x => new
            {
                x.Id,
                x.MemberId,
                x.MembershipId,
                x.Amount,
                x.PaidAt,
                x.Method,
                x.Reference,
                x.Status,
                x.Notes,
                x.CreatedByUserId,
                x.CreatedAt,
                x.UpdatedAt,
                CreatedByUser = x.CreatedByUser == null ? null : new
                {
                    x.CreatedByUser.Id,
                    x.CreatedByUser.Name,
                    x.CreatedByUser.Email
                },
                Member = new
                {
                    x.Member.Id,
                    x.Member.Name,
                    x.Member.Email
                }
            })
            .ToListAsync(ct);

        return Ok(payments);
    }

    [HasPermission(PermissionCodes.FeesView)]
    [HttpGet("member/{memberId:int}")]
    public Task<IActionResult> GetMember(int memberId, CancellationToken ct) => Get(memberId, ct);

    [HasPermission(PermissionCodes.FeesCreate)]
    [HttpPost]
    public async Task<IActionResult> Create(PaymentRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.Method) || !await db.Members.AnyAsync(x => x.Id == request.MemberId, ct))
            return BadRequest(new { message = "Valid member, amount and payment method are required." });
        if (request.PaidAt.HasValue && request.PaidAt.Value.Date > DateTime.UtcNow.Date)
            return BadRequest(new { message = "Payment date cannot be in the future." });
        if (request.MembershipId.HasValue && !await db.Memberships.AnyAsync(x => x.Id == request.MembershipId && x.MemberId == request.MemberId, ct))
            return BadRequest(new { message = "Membership was not found for this member." });
        var payment = new Payment { MemberId = request.MemberId, MembershipId = request.MembershipId, Amount = request.Amount, Method = request.Method.Trim(), Reference = request.Reference?.Trim() ?? "", Status = string.IsNullOrWhiteSpace(request.Status) ? "Paid" : request.Status.Trim(), Notes = request.Notes?.Trim() ?? "", PaidAt = request.PaidAt ?? DateTime.UtcNow, CreatedByUserId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : null };
        db.Payments.Add(payment); await db.SaveChangesAsync(ct);
        await hub.Clients.All.SendAsync("FeeUpdated", new { payment.Id, payment.MemberId }, ct);
        return CreatedAtAction(nameof(Get), new { id = payment.Id }, payment);
    }

    [HasPermission(PermissionCodes.FeesEdit)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PaymentRequest request, CancellationToken ct)
    {
        var payment = await db.Payments.FindAsync([id], ct);
        if (payment is null) return NotFound();
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.Method)) return BadRequest(new { message = "Valid amount and payment method are required." });
        if (request.PaidAt.HasValue && request.PaidAt.Value.Date > DateTime.UtcNow.Date)
            return BadRequest(new { message = "Payment date cannot be in the future." });
        payment.Amount = request.Amount; payment.PaidAt = request.PaidAt ?? payment.PaidAt; payment.Method = request.Method.Trim();
        payment.Reference = request.Reference?.Trim() ?? ""; payment.Status = string.IsNullOrWhiteSpace(request.Status) ? payment.Status : request.Status.Trim();
        payment.Notes = request.Notes?.Trim() ?? ""; payment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await hub.Clients.All.SendAsync("FeeUpdated", new { payment.Id, payment.MemberId }, ct);
        return Ok(payment);
    }
}
