using AeroMes.Application.Auth;
using AeroMes.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace AeroMes.Api.Auth;

public class PermissionAuthorizationHandler(
    IPermissionService permissionService,
    IAuditLogger auditLogger,
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? context.User.FindFirst("sub")?.Value;

        if (userId is null)
        {
            context.Fail();
            return;
        }

        if (await permissionService.HasPermissionAsync(userId, requirement.PermissionCode))
        {
            context.Succeed(requirement);
        }
        else
        {
            var ctx = httpContextAccessor.HttpContext;
            auditLogger.Log(new SecurityAuditEvent
            {
                EventType = AuditEventTypes.PermissionDenied,
                ActorId = userId,
                ActorType = "USER",
                ActorIp = ctx?.Connection.RemoteIpAddress?.ToString(),
                TargetType = "Permission",
                TargetId = requirement.PermissionCode,
                Outcome = "FAILURE",
                FailureReason = $"Missing permission: {requirement.PermissionCode}",
            });
            context.Fail();
        }
    }
}
