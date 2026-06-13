using AeroMes.Application.Auth;
using AeroMes.Application.Interfaces;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.PermissionOverrides.Commands.RemovePermissionOverride;

public class RemovePermissionOverrideHandler(
    IPermissionOverrideRepository repo,
    IPermissionService permissionService,
    IUnitOfWork uow,
    IAuditLogger auditLogger)
    : ICommandHandler<RemovePermissionOverrideCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemovePermissionOverrideCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.OverrideId, cmd.UserId, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"PermissionOverride '{cmd.OverrideId}' was not found.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
        await permissionService.InvalidateCacheAsync(cmd.UserId);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.PermissionOverrideRevoked,
            ActorId = cmd.ActorId, ActorType = "USER",
            TargetType = "User", TargetId = cmd.UserId,
            OldValues = $"{{\"permission\":\"{entity.Permission?.PermissionCode}\"}}",
        });
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
