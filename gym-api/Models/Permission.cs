namespace GymApi.Models;

public class Permission
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
