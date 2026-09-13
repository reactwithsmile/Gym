using GymApi.Data;
using GymApi.Hubs;
using GymApi.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Services;

public sealed class MembershipExpiryService(IServiceScopeFactory scopes, IHubContext<NotificationHub> hub, ILogger<MembershipExpiryService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CheckAsync(stoppingToken);
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await CheckAsync(stoppingToken);
    }

    private async Task CheckAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = DateTime.UtcNow;
            var memberships = await db.Memberships.Include(x => x.Member)
                .Where(x => x.Status == "Active" && x.EndDate <= now.AddDays(7)).ToListAsync(ct);
            foreach (var membership in memberships)
            {
                var days = (membership.EndDate.Date - now.Date).Days;
                var rule = days switch { 7 => "7", 3 => "3", 1 => "1", <= 0 => "expired", _ => null };
                if (rule is null) continue;
                var key = $"membership-expiring:{membership.Id}:{rule}";
                if (await db.Notifications.AnyAsync(x => x.DeduplicationKey == key, ct)) continue;
                var message = rule switch
                {
                    "7" => "Your gym membership expires in 7 days. Please renew your membership.",
                    "3" => "Your gym membership expires in 3 days. Please renew your membership.",
                    "1" => "Your gym membership expires tomorrow. Please renew your membership.",
                    _ => "Your gym membership has expired. Please renew your membership."
                };
                var notification = new Notification
                {
                    Title = rule == "expired" ? "Membership expired" : "Membership expiring",
                    Message = $"{membership.Member.Name}: {message}",
                    Type = "MembershipExpiring",
                    DeduplicationKey = key
                };
                db.Notifications.Add(notification);
                await db.SaveChangesAsync(ct);
                if (rule == "expired") membership.Status = "Expired";
                await db.SaveChangesAsync(ct);
                await hub.Clients.All.SendAsync("MembershipExpiring", new { membership, notification }, ct);
                await hub.Clients.All.SendAsync("NotificationCreated", notification, ct);
            }

            var expired = await db.Memberships.Where(x => x.Status == "Active" && x.EndDate < now).ToListAsync(ct);
            foreach (var membership in expired) membership.Status = "Expired";
            if (expired.Count > 0) await db.SaveChangesAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex) { logger.LogError(ex, "Membership expiry check failed."); }
    }
}
