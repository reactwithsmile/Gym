namespace GymApi.DTOs;

public class UpdateRolePermissionsRequest
{
    public List<string> PermissionCodes { get; set; } = [];
}
