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
        await SeedCmsDemoContentAsync(db, cancellationToken);
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
            ,[PermissionCodes.ProductsView] = "View products"
            ,[PermissionCodes.ProductsCreate] = "Create products"
            ,[PermissionCodes.ProductsEdit] = "Edit products"
            ,[PermissionCodes.ProductsDelete] = "Delete products"
            ,[PermissionCodes.MemberSpotlightView] = "View member spotlights"
            ,[PermissionCodes.MemberSpotlightCreate] = "Create member spotlights"
            ,[PermissionCodes.MemberSpotlightEdit] = "Edit member spotlights"
            ,[PermissionCodes.MemberSpotlightDelete] = "Delete member spotlights"
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
        var email = config["Seed:AdminEmail"]
            ?? throw new InvalidOperationException("Seed:AdminEmail must be configured when seeding is enabled.");
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
        var password = config["Seed:AdminPassword"]
            ?? throw new InvalidOperationException("Seed:AdminPassword must be configured when seeding is enabled.");
        user.PasswordHash = passwordHasher.HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedTrainerUserAsync(
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        IConfiguration config,
        CancellationToken cancellationToken)
    {
        var email = config["Seed:TrainerEmail"]
            ?? throw new InvalidOperationException("Seed:TrainerEmail must be configured when seeding is enabled.");
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

        var password = config["Seed:TrainerPassword"]
            ?? throw new InvalidOperationException("Seed:TrainerPassword must be configured when seeding is enabled.");
        user.PasswordHash = passwordHasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCmsDemoContentAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var settings = await db.GymSettings.OrderBy(item => item.Id).FirstOrDefaultAsync(cancellationToken);
        if (settings is null)
        {
            db.GymSettings.Add(new GymSettings
            {
                GymName = "G GYM",
                Currency = "INR",
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else if (string.IsNullOrWhiteSpace(settings.GymName) || settings.GymName.Equals("Gym", StringComparison.OrdinalIgnoreCase))
        {
            settings.GymName = "G GYM";
            settings.UpdatedAt = now;
        }
        if (string.IsNullOrWhiteSpace(settings.Currency) || settings.Currency.Trim().Equals("INR 100", StringComparison.OrdinalIgnoreCase))
        {
            settings.Currency = "INR";
            settings.UpdatedAt = now;
        }

        var hero = await db.Heroes.OrderByDescending(item => item.IsActive).ThenBy(item => item.Id).FirstOrDefaultAsync(cancellationToken);
        if (hero is null)
        {
            db.Heroes.Add(new Hero
            {
                Heading = "BUILD YOUR STRONGEST SELF.",
                Description = "Train with purpose. Build strength, improve your fitness and become more confident with every session.",
                PrimaryButtonText = "JOIN NOW",
                PrimaryButtonLink = "#membership",
                SecondaryButtonText = "EXPLORE THE GYM",
                SecondaryButtonLink = "#services",
                ImageUrl = string.Empty,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            hero.Heading = "BUILD YOUR STRONGEST SELF.";
            hero.Description = "Train with purpose. Build strength, improve your fitness and become more confident with every session.";
            hero.PrimaryButtonText = "JOIN NOW";
            hero.PrimaryButtonLink = "#membership";
            hero.SecondaryButtonText = "EXPLORE THE GYM";
            hero.SecondaryButtonLink = "#services";
            hero.UpdatedAt = now;
        }

        var about = await db.Abouts.OrderBy(item => item.Id).FirstOrDefaultAsync(cancellationToken);
        if (about is null)
        {
            db.Abouts.Add(new About
            {
                Title = "BUILT FOR THE WORK.",
                Subtitle = "Train smarter. Get stronger. Stay consistent.",
                Description = "G GYM is built for people who are serious about becoming stronger, fitter and more confident. With structured training, experienced coaches and a motivating community, every session has a purpose.",
                ExperienceYears = 8,
                MembersCount = 1200,
                TrainersCount = 12,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            about.Title = "BUILT FOR THE WORK.";
            about.Subtitle = "Train smarter. Get stronger. Stay consistent.";
            about.Description = "G GYM is built for people who are serious about becoming stronger, fitter and more confident. With structured training, experienced coaches and a motivating community, every session has a purpose.";
            about.ExperienceYears = 8;
            about.MembersCount = 1200;
            about.TrainersCount = 12;
            about.UpdatedAt = now;
        }

        var serviceContent = new[]
        {
            ("STRENGTH TRAINING", "Build strength with structured progressive training.", "Focused strength programs designed around proper technique, progressive overload and measurable progress."),
            ("PERSONAL TRAINING", "One-to-one coaching built around your goals.", "Get personalized workouts, coaching and accountability from experienced trainers who understand your goals."),
            ("FAT LOSS & CONDITIONING", "Build fitness, burn fat and move better.", "Structured conditioning and strength sessions designed to improve fitness, energy and body composition."),
            ("FUNCTIONAL FITNESS", "Train for strength that carries into everyday life.", "Improve mobility, endurance, balance and full-body strength through practical functional training.")
        };
        var services = await db.Services.OrderBy(item => item.DisplayOrder).ThenBy(item => item.Id).Take(serviceContent.Length).ToListAsync(cancellationToken);
        for (var index = 0; index < serviceContent.Length; index++)
        {
            var content = serviceContent[index];
            var service = index < services.Count ? services[index] : new Service
            {
                DisplayOrder = index + 1,
                IsActive = true,
                CreatedAt = now
            };
            service.Name = content.Item1;
            service.ShortDescription = content.Item2;
            service.Description = content.Item3;
            service.UpdatedAt = now;
            if (service.Id == 0) db.Services.Add(service);
        }

        var trainerContent = new[]
        {
            ("ARJUN MEHTA", "HEAD STRENGTH COACH", "Strength coach focused on building disciplined training habits and long-term performance.", "Strength Training", 8),
            ("RIYA SHAH", "FITNESS COACH", "Helps members build sustainable fitness routines with practical training and consistent coaching.", "Fat Loss & Conditioning", 6),
            ("KARAN PATEL", "PERSONAL TRAINER", "Focused on technique, functional movement and personalized training programs.", "Personal Training", 5)
        };
        var trainers = await db.Trainers.OrderBy(item => item.DisplayOrder).ThenBy(item => item.Id).Take(trainerContent.Length).ToListAsync(cancellationToken);
        for (var index = 0; index < trainerContent.Length; index++)
        {
            var content = trainerContent[index];
            var trainer = index < trainers.Count ? trainers[index] : new Trainer
            {
                DisplayOrder = index + 1,
                IsActive = true,
                CreatedAt = now
            };
            trainer.Name = content.Item1;
            trainer.Role = content.Item2;
            trainer.Bio = content.Item3;
            trainer.Specialization = content.Item4;
            trainer.ExperienceYears = content.Item5;
            trainer.UpdatedAt = now;
            if (trainer.Id == 0) db.Trainers.Add(trainer);
        }

        var planContent = new[]
        {
            ("STARTER", "Perfect for building a consistent training routine.", "Access to the gym and essential training facilities.", 1499m, 1, "Gym Access\nLocker Access\nFitness Assessment", false),
            ("PRO", "For members ready to train with purpose.", "A complete training experience with additional coaching support.", 3499m, 3, "Gym Access\nFitness Assessment\nWorkout Guidance\nProgress Tracking", true),
            ("ELITE", "Maximum support for serious results.", "Premium membership designed for members who want personalized guidance and accountability.", 8999m, 12, "Unlimited Gym Access\nPersonalized Workout Plan\nProgress Tracking\nTrainer Guidance\nPriority Support", false)
        };
        var plans = await db.MembershipPlans.OrderBy(item => item.DisplayOrder).ThenBy(item => item.Id).Take(planContent.Length).ToListAsync(cancellationToken);
        for (var index = 0; index < planContent.Length; index++)
        {
            var content = planContent[index];
            var plan = index < plans.Count ? plans[index] : new MembershipPlan
            {
                DisplayOrder = index + 1,
                IsActive = true,
                CreatedAt = now
            };
            plan.Name = content.Item1;
            plan.ShortDescription = content.Item2;
            plan.Description = content.Item3;
            plan.Price = content.Item4;
            plan.DurationMonths = content.Item5;
            plan.Features = content.Item6;
            plan.IsPopular = content.Item7;
            plan.UpdatedAt = now;
            if (plan.Id == 0) db.MembershipPlans.Add(plan);
        }

        var testimonialContent = new[]
        {
            ("Rahul", "Member for 2 years", "The biggest difference for me was the consistency. The trainers actually pay attention to technique and progress.", 5),
            ("Priya", "Member for 1 year", "Great environment, friendly trainers and proper guidance. I finally enjoy being consistent with my workouts.", 5),
            ("Dev", "Member for 8 months", "The atmosphere is motivating without feeling intimidating. Every workout feels productive.", 5)
        };
        var testimonials = await db.Testimonials.OrderBy(item => item.DisplayOrder).ThenBy(item => item.Id).Take(testimonialContent.Length).ToListAsync(cancellationToken);
        for (var index = 0; index < testimonialContent.Length; index++)
        {
            var content = testimonialContent[index];
            var testimonial = index < testimonials.Count ? testimonials[index] : new Testimonial
            {
                DisplayOrder = index + 1,
                IsActive = true,
                CreatedAt = now
            };
            testimonial.CustomerName = content.Item1;
            testimonial.RoleOrDescription = content.Item2;
            testimonial.Review = content.Item3;
            testimonial.Rating = content.Item4;
            testimonial.UpdatedAt = now;
            if (testimonial.Id == 0) db.Testimonials.Add(testimonial);
        }

        var galleryTitles = new[]
        {
            ("Strength Floor", "Strength", "A focused training floor built for consistent progress."),
            ("Training Session", "Training", "Structured sessions, focused effort and steady progress."),
            ("Functional Training", "Functional", "Practical movement and full-body conditioning."),
            ("Coach In Action", "Coaching", "Expert guidance where technique meets performance."),
            ("Gym Community", "Community", "A motivating environment built around showing up."),
            ("Training Together", "Community", "The energy of training with people who keep you moving.")
        };
        var gallery = await db.Gallery.OrderBy(item => item.DisplayOrder).ThenBy(item => item.Id).Take(galleryTitles.Length).ToListAsync(cancellationToken);
        for (var index = 0; index < gallery.Count && index < galleryTitles.Length; index++)
        {
            gallery[index].Title = galleryTitles[index].Item1;
            gallery[index].Category = galleryTitles[index].Item2;
            gallery[index].Description = galleryTitles[index].Item3;
            gallery[index].UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
