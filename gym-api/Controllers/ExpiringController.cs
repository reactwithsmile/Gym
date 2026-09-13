using GymApi.Auth;
using GymApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController, Route("api/expiring")]
public class ExpiringController(AppDbContext db) : ControllerBase
{
    [HasPermission(PermissionCodes.FeesView)]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int days = 7, CancellationToken ct = default)
    {
        days = Math.Clamp(days, 1, 365);
        var today = DateTime.UtcNow.Date;
        var until = today.AddDays(days);
        var since = today.AddDays(-7);
        var memberships = await db.Memberships.Include(x => x.Member).Include(x => x.MembershipPlan)
            .Where(x => (x.Status == "Active" || x.Status == "Expired") && x.EndDate.Date >= since && x.EndDate.Date <= until)
            .OrderBy(x => x.EndDate).ToListAsync(ct);
        return Ok(memberships.Select(item => new
        {
            membershipId = item.Id,
            memberId = item.MemberId,
            memberName = item.Member.Name,
            planName = item.MembershipPlan?.Name ?? "Membership",
            expiryDate = item.EndDate,
            daysRemaining = Math.Max(0, (item.EndDate.Date - today).Days),
            status = item.EndDate.Date < today ? "Expired" : item.EndDate.Date <= today.AddDays(7) ? "Expiring Soon" : "Active"
        }));
    }
}
