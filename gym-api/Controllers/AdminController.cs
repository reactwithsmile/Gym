using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    [HasPermission(PermissionCodes.RolesView)]
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _db.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .ToListAsync(cancellationToken);

        var response = roles.Select(r => new
        {
            r.Id,
            r.Name,
            Permissions = r.RolePermissions
                .Select(rp => new { rp.Permission.Id, rp.Permission.Code, rp.Permission.Name })
                .OrderBy(p => p.Code)
                .ToList(),
            PermissionCodes = r.RolePermissions
                .Select(rp => rp.Permission.Code)
                .OrderBy(code => code)
                .ToList()
        });

        return Ok(response);
    }

    [HasPermission(PermissionCodes.RolesEdit)]
    [HttpPut("roles/{roleId:int}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(
        int roleId,
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
        if (role is null)
        {
            return NotFound(new { message = $"Role with id {roleId} was not found." });
        }

        var submittedCodes = request.PermissionCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (role.Name == RoleNames.Admin)
        {
            submittedCodes = PermissionCodes.All.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        var existingCodes = await _db.Permissions
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        var missingCodes = submittedCodes
            .Except(existingCodes, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (missingCodes.Count > 0)
        {
            return BadRequest(new { message = "One or more permission codes are invalid.", invalidCodes = missingCodes });
        }

        var permissionMap = await _db.Permissions
            .Where(p => submittedCodes.Contains(p.Code))
            .ToDictionaryAsync(p => p.Code, p => p.Id, cancellationToken);

        var existingLinks = await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        if (existingLinks.Count > 0)
        {
            _db.RolePermissions.RemoveRange(existingLinks);
        }

        foreach (var code in submittedCodes)
        {
            _db.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionMap[code]
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        var updatedRole = await _db.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstAsync(r => r.Id == roleId, cancellationToken);

        var response = new
        {
            updatedRole.Id,
            updatedRole.Name,
            Permissions = updatedRole.RolePermissions
                .Select(rp => new { rp.Permission.Id, rp.Permission.Code, rp.Permission.Name })
                .OrderBy(p => p.Code)
                .ToList(),
            PermissionCodes = updatedRole.RolePermissions
                .Select(rp => rp.Permission.Code)
                .OrderBy(code => code)
                .ToList()
        };

        return Ok(response);
    }
}
