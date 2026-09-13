using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Hubs;
using GymApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController, Route("api/notifications")]
public class NotificationsController(AppDbContext db, IHubContext<NotificationHub> hub) : ControllerBase
{
    [HasPermission(PermissionCodes.NotificationsView)]
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await db.Notifications.OrderByDescending(x => x.CreatedAt).ToListAsync(ct));

    [HasPermission(PermissionCodes.NotificationsEdit)]
    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        var item = await db.Notifications.FindAsync([id], ct);
        if (item is null) return NotFound();
        item.IsRead = true; await db.SaveChangesAsync(ct); return Ok(item);
    }

    [HasPermission(PermissionCodes.NotificationsEdit)]
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await db.Notifications.Where(item => !item.IsRead).ExecuteUpdateAsync(setters => setters.SetProperty(item => item.IsRead, true), ct);
        return NoContent();
    }

    [HasPermission(PermissionCodes.NotificationsEdit)]
    [HttpPost]
    public async Task<IActionResult> Create(NotificationRequest request, CancellationToken ct)
    {
        var item = new Notification { Title = request.Title.Trim(), Message = request.Message.Trim(), Type = request.Type?.Trim() ?? "General" };
        if (string.IsNullOrWhiteSpace(item.Title) || string.IsNullOrWhiteSpace(item.Message)) return BadRequest();
        db.Notifications.Add(item); await db.SaveChangesAsync(ct);
        await hub.Clients.All.SendAsync("NotificationCreated", item, ct);
        return Ok(item);
    }
}
