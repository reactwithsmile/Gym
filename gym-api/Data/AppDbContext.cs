using GymApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Hero> Heroes => Set<Hero>();
    public DbSet<About> Abouts => Set<About>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Gallery> Gallery => Set<Gallery>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<GymSettings> GymSettings => Set<GymSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(r => r.Name).IsUnique();
            entity.Property(r => r.Name).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(p => p.Code).HasMaxLength(64).IsRequired();
            entity.Property(p => p.Name).HasMaxLength(128).IsRequired();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);
            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
        });

        modelBuilder.Entity<Hero>(entity =>
        {
            entity.Property(h => h.Heading).HasMaxLength(200).IsRequired();
            entity.Property(h => h.Description).HasMaxLength(1000).IsRequired();
            entity.Property(h => h.PrimaryButtonText).HasMaxLength(100);
            entity.Property(h => h.PrimaryButtonLink).HasMaxLength(500);
            entity.Property(h => h.SecondaryButtonText).HasMaxLength(100);
            entity.Property(h => h.SecondaryButtonLink).HasMaxLength(500);
            entity.Property(h => h.ImageUrl).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(h => h.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(h => h.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(h => h.IsActive);
        });

        modelBuilder.Entity<About>(entity =>
        {
            entity.Property(a => a.Title).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Subtitle).HasMaxLength(300);
            entity.Property(a => a.Description).HasMaxLength(2000).IsRequired();
            entity.Property(a => a.ImageUrl).HasColumnType("nvarchar(max)");
            entity.HasIndex(a => a.IsActive);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(service => service.Name).HasMaxLength(200).IsRequired();
            entity.Property(service => service.ShortDescription).HasMaxLength(500).IsRequired();
            entity.Property(service => service.Description).HasMaxLength(2000);
            entity.Property(service => service.ImageUrl).HasColumnType("nvarchar(max)");
            entity.Property(service => service.Icon).HasMaxLength(100);
            entity.HasIndex(service => new { service.IsActive, service.DisplayOrder });
        });

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.Property(trainer => trainer.Name).HasMaxLength(200).IsRequired();
            entity.Property(trainer => trainer.Role).HasMaxLength(200).IsRequired();
            entity.Property(trainer => trainer.Bio).HasMaxLength(2000);
            entity.Property(trainer => trainer.ImageUrl).HasColumnType("nvarchar(max)");
            entity.Property(trainer => trainer.Specialization).HasMaxLength(300);
            entity.Property(trainer => trainer.InstagramUrl).HasMaxLength(500);
            entity.Property(trainer => trainer.FacebookUrl).HasMaxLength(500);
            entity.HasIndex(trainer => new { trainer.IsActive, trainer.DisplayOrder });
        });

        modelBuilder.Entity<MembershipPlan>(entity =>
        {
            entity.Property(plan => plan.Name).HasMaxLength(200).IsRequired();
            entity.Property(plan => plan.ShortDescription).HasMaxLength(500).IsRequired();
            entity.Property(plan => plan.Description).HasMaxLength(2000);
            entity.Property(plan => plan.Price).HasPrecision(18, 2);
            entity.Property(plan => plan.Features).HasMaxLength(2000);
            entity.HasIndex(plan => new { plan.IsActive, plan.DisplayOrder });
        });

        modelBuilder.Entity<Gallery>(entity =>
        {
            entity.Property(item => item.Title).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(2000);
            entity.Property(item => item.ImageUrl).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(item => item.Category).HasMaxLength(100);
            entity.HasIndex(item => new { item.IsActive, item.DisplayOrder });
        });

        modelBuilder.Entity<Testimonial>(entity =>
        {
            entity.Property(item => item.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(item => item.RoleOrDescription).HasMaxLength(300);
            entity.Property(item => item.Review).HasMaxLength(2000).IsRequired();
            entity.Property(item => item.ImageUrl).HasColumnType("nvarchar(max)");
            entity.HasIndex(item => new { item.IsActive, item.DisplayOrder });
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.Property(item => item.GymName).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Address).HasMaxLength(500);
            entity.Property(item => item.Phone).HasMaxLength(100);
            entity.Property(item => item.Email).HasMaxLength(256);
            entity.Property(item => item.OpeningHours).HasMaxLength(500);
            entity.Property(item => item.GoogleMapsUrl).HasMaxLength(1000);
            entity.Property(item => item.InstagramUrl).HasMaxLength(500);
            entity.Property(item => item.FacebookUrl).HasMaxLength(500);
            entity.Property(item => item.WhatsAppNumber).HasMaxLength(100);
            entity.Property(item => item.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<GymSettings>(entity =>
        {
            entity.Property(item => item.GymName).HasMaxLength(200).IsRequired();
            entity.Property(item => item.LogoUrl).HasColumnType("nvarchar(max)");
            entity.Property(item => item.Tagline).HasMaxLength(300);
            entity.Property(item => item.WebsiteUrl).HasMaxLength(1000);
            entity.Property(item => item.Currency).HasMaxLength(10).IsRequired();
        });
    }
}
