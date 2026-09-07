using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AdminUsersController(AppDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [HasPermission(PermissionCodes.UsersView)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _db.Users.Include(user => user.Role).OrderBy(user => user.Name).ThenBy(user => user.Id).ToListAsync(cancellationToken);
        return Ok(users.Select(ToDto).ToList());
    }

    [HasPermission(PermissionCodes.UsersView)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminUserDto>> Get(int id, CancellationToken cancellationToken)
    {
        var user = await _db.Users.Include(item => item.Role).FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return user is null ? NotFound(new { message = $"User with id {id} was not found." }) : Ok(ToDto(user));
    }

    [HasPermission(PermissionCodes.UsersCreate)]
    [HttpPost]
    public async Task<ActionResult<AdminUserDto>> Create(CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateRequest(request.Name, request.Email, request.Password, request.RoleId, cancellationToken);
        if (validation is not null) return BadRequest(new { message = validation });
        var email = request.Email.Trim();
        if (await _db.Users.AnyAsync(user => user.Email == email, cancellationToken)) return Conflict(new { message = "A user with this email already exists." });
        var user = new User { Name = request.Name.Trim(), Email = email, RoleId = request.RoleId, IsActive = request.IsActive, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        await _db.Entry(user).Reference(item => item.Role).LoadAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, ToDto(user));
    }

    [HasPermission(PermissionCodes.UsersEdit)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminUserDto>> Update(int id, UpdateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.Include(item => item.Role).FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null) return NotFound(new { message = $"User with id {id} was not found." });
        var validation = await ValidateRequest(request.Name, request.Email, request.Password, request.RoleId, cancellationToken, false);
        if (validation is not null) return BadRequest(new { message = validation });
        var email = request.Email.Trim();
        if (await _db.Users.AnyAsync(item => item.Id != id && item.Email == email, cancellationToken)) return Conflict(new { message = "A user with this email already exists." });
        if (user.Role.Name == RoleNames.Admin && (!request.IsActive || request.RoleId != user.RoleId) && !await HasAnotherActiveAdmin(user.Id, cancellationToken))
            return Conflict(new { message = "The last active Admin account cannot be deactivated or reassigned." });
        user.Name = request.Name.Trim(); user.Email = email; user.RoleId = request.RoleId; user.IsActive = request.IsActive; user.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Password)) user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        await _db.SaveChangesAsync(cancellationToken);
        await _db.Entry(user).Reference(item => item.Role).LoadAsync(cancellationToken);
        return Ok(ToDto(user));
    }

    [HasPermission(PermissionCodes.UsersDelete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var user = await _db.Users.Include(item => item.Role).FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null) return NotFound(new { message = $"User with id {id} was not found." });
        if (user.IsActive && user.Role.Name == RoleNames.Admin && !await HasAnotherActiveAdmin(user.Id, cancellationToken))
            return Conflict(new { message = "The last active Admin account cannot be deactivated." });
        user.IsActive = false; user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<string?> ValidateRequest(string name, string email, string? password, int roleId, CancellationToken cancellationToken, bool passwordRequired = true)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Name is required.";
        try { _ = new System.Net.Mail.MailAddress(email.Trim()); } catch (FormatException) { return "Email format is invalid."; }
        if (passwordRequired && string.IsNullOrWhiteSpace(password)) return "Password is required.";
        if (!string.IsNullOrWhiteSpace(password) && password.Length < 8) return "Password must be at least 8 characters.";
        if (!await _db.Roles.AnyAsync(role => role.Id == roleId, cancellationToken)) return "Selected role was not found.";
        return null;
    }

    private Task<bool> HasAnotherActiveAdmin(int excludedUserId, CancellationToken cancellationToken) =>
        _db.Users.AnyAsync(user => user.Id != excludedUserId && user.IsActive && user.Role.Name == RoleNames.Admin, cancellationToken);

    private static AdminUserDto ToDto(User user) => new() { Id = user.Id, Name = user.Name, Email = user.Email, RoleId = user.RoleId, RoleName = user.Role.Name, IsActive = user.IsActive, CreatedAt = user.CreatedAt, UpdatedAt = user.UpdatedAt };
}
