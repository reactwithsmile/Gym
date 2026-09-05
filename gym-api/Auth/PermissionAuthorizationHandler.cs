using Microsoft.AspNetCore.Authorization;

namespace GymApi.Auth;

public class PermissionAuthorizationHandler : AuthorizationHandler<HasPermissionAttribute>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasPermissionAttribute requirement)
    {
        if (context.User.IsInRole(RoleNames.Admin) ||
            context.User.HasClaim("permission", requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
