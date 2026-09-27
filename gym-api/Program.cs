using System.Text;
using System.Linq;
using GymApi.Auth;
using GymApi.Data;
using GymApi.Models;
using GymApi.Services;
using GymApi.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration is missing.");
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key must be configured with at least 32 characters.");
}

if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience))
{
    throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience must be configured.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHostedService<MembershipExpiryService>();
builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddCors(options =>
{
    var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
    if (origins.Length == 0)
    {
        throw new InvalidOperationException("Cors:Origins must contain at least one allowed frontend origin.");
    }

    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var initialAdminProvisioningEnabled =
        builder.Configuration.GetValue<bool>("InitialAdminProvisioning:Enabled");
    var broadSeedingEnabled = builder.Configuration.GetValue<bool>("Seed:Enabled");
    var autoMigrateEnabled = builder.Configuration.GetValue<bool>("Database:AutoMigrate");

    if (initialAdminProvisioningEnabled && !app.Environment.IsProduction())
    {
        throw new InvalidOperationException(
            "Initial admin provisioning can only run when ASPNETCORE_ENVIRONMENT is Production.");
    }

    if (initialAdminProvisioningEnabled && autoMigrateEnabled)
    {
        throw new InvalidOperationException(
            "Database:AutoMigrate must be false when initial admin provisioning is enabled.");
    }

    if (app.Environment.IsProduction() && broadSeedingEnabled)
    {
        throw new InvalidOperationException(
            "Seed:Enabled must remain false in Production. Use the one-time initial admin provisioning flag instead.");
    }

    if (initialAdminProvisioningEnabled && broadSeedingEnabled)
    {
        throw new InvalidOperationException(
            "Initial admin provisioning and broad database seeding cannot run together.");
    }

    if (autoMigrateEnabled)
    {
        await db.Database.MigrateAsync();
    }

    if (initialAdminProvisioningEnabled)
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("InitialAdminProvisioning");
        await InitialAdminProvisioner.ProvisionAsync(
            db,
            passwordHasher,
            builder.Configuration,
            logger);
    }

    if (broadSeedingEnabled)
    {
        await DbSeeder.SeedAsync(scope.ServiceProvider);
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseRouting();
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PORT")))
{
    app.UseHttpsRedirection();
}
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.Run();
