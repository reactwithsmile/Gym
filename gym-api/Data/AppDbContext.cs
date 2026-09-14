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
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Enquiry> Enquiries => Set<Enquiry>();
    public DbSet<Product> Products => Set<Product>();

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

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasOne(member => member.CreatedByUser)
                .WithMany()
                .HasForeignKey(member => member.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(member => member.UpdatedByUser)
                .WithMany()
                .HasForeignKey(member => member.UpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Name).HasMaxLength(200).IsRequired();
            entity.Property(product => product.Category).HasMaxLength(100).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(2000);
            entity.Property(product => product.ImageUrl).HasColumnType("nvarchar(max)");
            entity.Property(product => product.Price).HasPrecision(18, 2);
            entity.HasIndex(product => new { product.IsActive, product.DisplayOrder });
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

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(item => item.Amount).HasPrecision(18, 2);
            entity.Property(item => item.Method).HasMaxLength(50).IsRequired();
            entity.Property(item => item.Reference).HasMaxLength(200);
            entity.Property(item => item.Status).HasMaxLength(32).IsRequired();
            entity.Property(item => item.Notes).HasMaxLength(1000);
            entity.HasOne(item => item.CreatedByUser).WithMany().HasForeignKey(item => item.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.HasIndex(x => x.Email);
        });
        modelBuilder.Entity<Membership>(entity =>
        {
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasOne(x => x.Member).WithMany(x => x.Memberships).HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.MembershipPlan).WithMany().HasForeignKey(x => x.MembershipPlanId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.EndDate, x.Status });
        });
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Method).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Reference).HasMaxLength(200);
            entity.HasOne(x => x.Member).WithMany(x => x.Payments).HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Membership).WithMany(x => x.Payments).HasForeignKey(x => x.MembershipId).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DeduplicationKey).HasMaxLength(300);
            entity.HasIndex(x => x.DeduplicationKey).IsUnique().HasFilter("[DeduplicationKey] IS NOT NULL");
        });
    }
}
