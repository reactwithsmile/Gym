using Microsoft.AspNetCore.Authorization;

namespace GymApi.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : Attribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
    public string Permission { get; }

    public HasPermissionAttribute(string permission)
    {
        Permission = permission;
    }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}
