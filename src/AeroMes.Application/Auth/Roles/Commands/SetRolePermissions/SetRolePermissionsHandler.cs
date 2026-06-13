using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.Roles.Commands.SetRolePermissions;

public class SetRolePermissionsHandler(
    IRoleRepository roles,
    IPermissionRepository repo,
    IUnitOfWork uow,
    IAuditLogger auditLogger)
    : ICommandHandler<SetRolePermissionsCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(SetRolePermissionsCommand cmd, CancellationToken ct)
    {
        if (!await roles.ExistsAsync(cmd.RoleId, ct))
            return ValidationResult<Unit>.NotFound($"Role '{cmd.RoleId}' was not found.");

        var existing = await repo.GetRolePermissionsAsync(cmd.RoleId, ct);
        repo.RemoveRolePermissions(existing);

        var permIds = await repo.GetIdsByCodesAsync(cmd.PermissionCodes, ct);
        foreach (var pid in permIds)
            repo.AddRolePermission(RolePermission.Create(cmd.RoleId, pid));

        await uow.SaveChangesAsync(ct);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.RolePermissionChanged,
            ActorId = cmd.ActorId, ActorType = "USER",
            TargetType = "Role", TargetId = cmd.RoleId,
            NewValues = string.Join(",", cmd.PermissionCodes),
        });
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
