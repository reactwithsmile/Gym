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
        await SeedTrainerUserAsync(db, passwordHasher, config, cancellationToken);
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

            [PermissionCodes.HeroView] = "View hero",
            [PermissionCodes.HeroCreate] = "Create hero",
            [PermissionCodes.HeroEdit] = "Edit hero",
            [PermissionCodes.HeroDelete] = "Delete hero",

            [PermissionCodes.AboutView] = "View about",
            [PermissionCodes.AboutCreate] = "Create about",
            [PermissionCodes.AboutEdit] = "Edit about",
            [PermissionCodes.AboutDelete] = "Delete about",

            [PermissionCodes.ServicesView] = "View services",
            [PermissionCodes.ServicesCreate] = "Create services",
            [PermissionCodes.ServicesEdit] = "Edit services",
            [PermissionCodes.ServicesDelete] = "Delete services",

            [PermissionCodes.TrainersView] = "View trainers",
            [PermissionCodes.TrainersCreate] = "Create trainers",
            [PermissionCodes.TrainersEdit] = "Edit trainers",
            [PermissionCodes.TrainersDelete] = "Delete trainers",

            [PermissionCodes.MembershipView] = "View membership plans",
            [PermissionCodes.MembershipCreate] = "Create membership plans",
            [PermissionCodes.MembershipEdit] = "Edit membership plans",
            [PermissionCodes.MembershipDelete] = "Delete membership plans",

            [PermissionCodes.GalleryView] = "View gallery",
            [PermissionCodes.GalleryCreate] = "Create gallery",
            [PermissionCodes.GalleryEdit] = "Edit gallery",
            [PermissionCodes.GalleryDelete] = "Delete gallery",

            [PermissionCodes.TestimonialsView] = "View testimonials",
            [PermissionCodes.TestimonialsCreate] = "Create testimonials",
            [PermissionCodes.TestimonialsEdit] = "Edit testimonials",
            [PermissionCodes.TestimonialsDelete] = "Delete testimonials",

            [PermissionCodes.ContactView] = "View contact",
            [PermissionCodes.ContactEdit] = "Edit contact",

            [PermissionCodes.SettingsView] = "View settings",
            [PermissionCodes.SettingsEdit] = "Edit settings",

            [PermissionCodes.RolesView] = "View roles and permissions",
            [PermissionCodes.RolesEdit] = "Edit roles and permissions"
            ,[PermissionCodes.UsersView] = "View users"
            ,[PermissionCodes.UsersCreate] = "Create users"
            ,[PermissionCodes.UsersEdit] = "Edit users"
            ,[PermissionCodes.UsersDelete] = "Delete users"
            ,[PermissionCodes.MembersView] = "View members"
            ,[PermissionCodes.MembersCreate] = "Create members"
            ,[PermissionCodes.MembersEdit] = "Edit members"
            ,[PermissionCodes.FeesView] = "View fees"
            ,[PermissionCodes.FeesCreate] = "Create fees"
            ,[PermissionCodes.FeesEdit] = "Edit fees"
            ,[PermissionCodes.NotificationsView] = "View notifications"
            ,[PermissionCodes.NotificationsEdit] = "Edit notifications"
            ,[PermissionCodes.EnquiriesView] = "View enquiries"
            ,[PermissionCodes.EnquiriesEdit] = "Edit enquiries"
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
            Name = "Administrator",
            Email = email,
            RoleId = adminRole.Id,
            IsActive = true
        };
        user.PasswordHash = passwordHasher.HashPassword(user, config["Seed:AdminPassword"] ?? "Admin123!");
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedTrainerUserAsync(
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        IConfiguration config,
        CancellationToken cancellationToken)
    {
        var email = config["Seed:TrainerEmail"] ?? "trainer@gym.local";
        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            return;
        }

        var trainerRole = await db.Roles.FirstAsync(r => r.Name == RoleNames.Trainer, cancellationToken);
        var user = new User
        {
            Name = "Trainer",
            Email = email,
            RoleId = trainerRole.Id,
            IsActive = true
        };

        user.PasswordHash = passwordHasher.HashPassword(user, config["Seed:TrainerPassword"] ?? "Trainer123!");

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }
}
