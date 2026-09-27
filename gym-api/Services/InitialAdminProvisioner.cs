using GymApi.Auth;
using GymApi.Data;
using GymApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Services;

public static class InitialAdminProvisioner
{
    public static async Task ProvisionAsync(
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var email = configuration["InitialAdminProvisioning:Email"]?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException(
                "InitialAdminProvisioning:Email must be configured when initial admin provisioning is enabled.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var existingUser = await db.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

        if (existingUser is not null)
        {
            if (existingUser.Role.Name != RoleNames.Admin)
            {
                throw new InvalidOperationException(
                    "A user with the configured initial admin email already exists and is not an Admin. No user was changed.");
            }

            logger.LogInformation(
                "Initial admin provisioning skipped because the configured Admin user already exists. No user was changed.");
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var password = configuration["InitialAdminProvisioning:Password"];
        if (string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(
                "InitialAdminProvisioning:Password must be configured when creating the initial Admin user.");
        }

        var adminRole = await db.Roles
            .FirstOrDefaultAsync(role => role.Name == RoleNames.Admin, cancellationToken);
        if (adminRole is null)
        {
            adminRole = new Role { Name = RoleNames.Admin };
            db.Roles.Add(adminRole);
            await db.SaveChangesAsync(cancellationToken);
        }

        var admin = new User
        {
            Name = "Administrator",
            Email = email,
            RoleId = adminRole.Id,
            IsActive = true
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, password);

        db.Users.Add(admin);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation("Initial Admin user provisioned successfully.");
    }
}
