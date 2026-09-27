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
        logger.LogInformation("Initial admin provisioning enabled.");

        var email = configuration["InitialAdminProvisioning:Email"]?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException(
                "InitialAdminProvisioning:Email must be configured when initial admin provisioning is enabled.");
        }

        var password = configuration["InitialAdminProvisioning:Password"];
        if (string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(
                "InitialAdminProvisioning:Password must be configured when initial admin provisioning is enabled.");
        }

        var normalizedEmail = email.ToUpperInvariant();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var existingUser = await db.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(
                user => user.Email.ToUpper() == normalizedEmail,
                cancellationToken);

        if (existingUser is not null)
        {
            if (existingUser.Role.Name != RoleNames.Admin)
            {
                throw new InvalidOperationException(
                    "A user with the configured initial admin email already exists and is not an Admin. No user was changed.");
            }

            existingUser.IsActive = true;
            existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, password);
            existingUser.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Existing admin user found; password reset performed.");
            return;
        }

        var adminRole = await db.Roles
            .FirstOrDefaultAsync(role => role.Name == RoleNames.Admin, cancellationToken);
        if (adminRole is null)
        {
            adminRole = new Role { Name = RoleNames.Admin };
            db.Roles.Add(adminRole);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Admin role created.");
        }
        else
        {
            logger.LogInformation("Admin role found.");
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

        logger.LogInformation("Initial admin user created.");
    }
}
