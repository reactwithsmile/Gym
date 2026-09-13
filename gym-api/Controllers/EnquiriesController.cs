using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Hubs;
using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController, Route("api/enquiries")]
public class EnquiriesController(AppDbContext db, IHubContext<NotificationHub> hub) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEnquiryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { message = "Name, email and message are required." });
        if (!System.Net.Mail.MailAddress.TryCreate(request.Email.Trim(), out _))
            return BadRequest(new { message = "Email format is invalid." });
        var enquiry = new Enquiry { Name = request.Name.Trim(), Email = request.Email.Trim(), Phone = request.Phone?.Trim() ?? "", Message = request.Message.Trim() };
        db.Enquiries.Add(enquiry);
        db.Notifications.Add(new Notification
        {
            Title = "New website enquiry",
            Message = $"{enquiry.Name} submitted an enquiry.",
            Type = "Enquiry",
            DeduplicationKey = $"enquiry:{enquiry.Id}:{DateTime.UtcNow.Ticks}"
        });
        await db.SaveChangesAsync(ct);
        await hub.Clients.All.SendAsync("NotificationCreated", new { enquiry.Id, enquiry.Name }, ct);
        await hub.Clients.All.SendAsync("EnquiryCreated", new { enquiry.Id, enquiry.Name, enquiry.CreatedAt }, ct);
        return CreatedAtAction(nameof(Get), new { id = enquiry.Id }, enquiry);
    }

    [HasPermission(PermissionCodes.EnquiriesView)]
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await db.Enquiries.OrderByDescending(x => x.CreatedAt).ToListAsync(ct));

    [HasPermission(PermissionCodes.EnquiriesEdit)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateEnquiryRequest request, CancellationToken ct)
    {
        var enquiry = await db.Enquiries.FindAsync([id], ct);
        if (enquiry is null) return NotFound(new { message = "Enquiry was not found." });
        var statuses = new[] { "New", "Contacted", "Converted", "Closed", "Spam" };
        if (!statuses.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Invalid enquiry status." });
        if (request.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { message = "A reason is required when closing an enquiry." });
        enquiry.Status = request.Status.Trim();
        enquiry.Reason = request.Reason?.Trim() ?? "";
        enquiry.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(enquiry);
    }

    [HasPermission(PermissionCodes.EnquiriesEdit)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var enquiry = await db.Enquiries.FindAsync([id], ct);
        if (enquiry is null) return NotFound();
        db.Enquiries.Remove(enquiry);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
