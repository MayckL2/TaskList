using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TaskList.Requirements;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        var user = context.User;

        if (user.Identity is not { IsAuthenticated: true })
        {
            _logger.LogDebug("User is not authenticated");
            return Task.CompletedTask;
        }

        // Check if user has the required permission claim
        var hasPermission = user.Claims.Any(c =>
            c.Type == "permission" && c.Value == requirement.Permission
        );

        // Also check for wildcard permissions (optional)
        var hasWildcard = user.Claims.Any(c =>
            c.Type == "permission"
            && (c.Value == "*" || c.Value == $"{requirement.Permission.Split(':')[0]}:*")
        );

        if (hasPermission || hasWildcard)
        {
            _logger.LogDebug(
                "User {User} has permission {Permission}",
                user.Identity.Name,
                requirement.Permission
            );
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogDebug(
                "User {User} does NOT have permission {Permission}",
                user.Identity.Name,
                requirement.Permission
            );
        }

        return Task.CompletedTask;
    }
}
