using GymApi.Auth;
using GymApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();
        var config = services.GetRequiredService<IConfiguration>();

        await SeedRolesAsync(db, cancellationToken);
        await SeedPermissionsAsync(db, cancellationToken);
        await SeedRolePermissionsAsync(db, cancellationToken);
        await SeedAdminUserAsync(db, passwordHasher, config, cancellationToken);
    }

    private static async Task SeedRolesAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        foreach (var name in new[] { RoleNames.Admin, RoleNames.Trainer })
        {
            if (!await db.Roles.AnyAsync(r => r.Name == name, cancellationToken))
            {
                db.Roles.Add(new Role { Name = name });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPermissionsAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var names = new Dictionary<string, string>
        {
            [PermissionCodes.DashboardView] = "View dashboard",
            [PermissionCodes.HeroManage] = "Manage hero",
            [PermissionCodes.AboutManage] = "Manage about",
            [PermissionCodes.ServicesManage] = "Manage services",
            [PermissionCodes.TrainersManage] = "Manage trainers",
            [PermissionCodes.MembershipManage] = "Manage membership plans",
            [PermissionCodes.GalleryManage] = "Manage gallery",
            [PermissionCodes.TestimonialsManage] = "Manage testimonials",
            [PermissionCodes.ContactManage] = "Manage contact",
            [PermissionCodes.SettingsManage] = "Manage settings",
            [PermissionCodes.RolesManage] = "Manage roles and permissions"
        };

        foreach (var (code, name) in names)
        {
            if (!await db.Permissions.AnyAsync(p => p.Code == code, cancellationToken))
            {
                db.Permissions.Add(new Permission { Code = code, Name = name });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRolePermissionsAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var admin = await db.Roles.FirstAsync(r => r.Name == RoleNames.Admin, cancellationToken);
        var trainer = await db.Roles.FirstAsync(r => r.Name == RoleNames.Trainer, cancellationToken);
        var permissions = await db.Permissions.ToListAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            await AddRolePermissionAsync(db, admin.Id, permission.Id, cancellationToken);
        }

        var trainerDefault = permissions.First(p => p.Code == PermissionCodes.DashboardView);
        await AddRolePermissionAsync(db, trainer.Id, trainerDefault.Id, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task AddRolePermissionAsync(
        AppDbContext db,
        int roleId,
        int permissionId,
        CancellationToken cancellationToken)
    {
        var exists = await db.RolePermissions.AnyAsync(
            rp => rp.RoleId == roleId && rp.PermissionId == permissionId,
            cancellationToken);

        if (!exists)
        {
            db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
        }
    }

    private static async Task SeedAdminUserAsync(
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        IConfiguration config,
        CancellationToken cancellationToken)
    {
        var email = config["Seed:AdminEmail"] ?? "admin@gym.local";
        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            return;
        }

        var adminRole = await db.Roles.FirstAsync(r => r.Name == RoleNames.Admin, cancellationToken);
        var user = new User
        {
            Email = email,
            RoleId = adminRole.Id,
            IsActive = true
        };
        user.PasswordHash = passwordHasher.HashPassword(user, config["Seed:AdminPassword"] ?? "Admin123!");
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }
}
