using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GymApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtSettings _jwt;

    public AuthService(
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        IOptions<JwtSettings> jwtOptions)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwtOptions.Value;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var user = await _db.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var dto = ToDto(user);
        return new LoginResponse
        {
            Token = CreateToken(user, dto.Permissions),
            User = dto
        };
    }

    public async Task<AuthUserDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        return user is null ? null : ToDto(user);
    }

    private static AuthUserDto ToDto(User user)
    {
        var permissions = user.Role.Name == RoleNames.Admin
            ? PermissionCodes.All
            : user.Role.RolePermissions.Select(rp => rp.Permission.Code).ToArray();

        return new AuthUserDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role.Name,
            Permissions = permissions
        };
    }

    private string CreateToken(User user, IReadOnlyList<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.Name)
        };

        claims.AddRange(permissions.Select(code => new Claim("permission", code)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiresInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
