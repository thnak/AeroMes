using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Domain.Exceptions;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Auth.PermissionOverrides.Commands.AddPermissionOverride;

public class AddPermissionOverrideHandler(
    IUserRepository users,
    IPermissionRepository permissionRepo,
    IPermissionOverrideRepository overrideRepo,
    IPermissionService permissionService,
    IUnitOfWork uow,
    IAuditLogger auditLogger,
    IValidator<AddPermissionOverrideCommand> validator)
    : ICommandHandler<AddPermissionOverrideCommand, ValidationResult<PermissionOverrideDto>>
{
    public async Task<ValidationResult<PermissionOverrideDto>> HandleAsync(AddPermissionOverrideCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<PermissionOverrideDto>.Invalid(validation.ToErrorDictionary());

        try
        {
            if (!await users.ExistsAsync(cmd.UserId, ct))
                throw new EntityNotFoundException("User", cmd.UserId);

            var permission = await permissionRepo.GetByCodeAsync(cmd.PermissionCode, ct)
                ?? throw new DomainException($"Unknown permission code: {cmd.PermissionCode}");

            var existing = await overrideRepo.FindAsync(cmd.UserId, permission.PermissionId, ct);
            if (existing is not null) overrideRepo.Remove(existing);

            var effect = cmd.Effect.Equals("Grant", StringComparison.OrdinalIgnoreCase)
                ? PermissionEffect.Grant : PermissionEffect.Deny;

            var entity = UserPermissionOverride.Create(cmd.UserId, permission.PermissionId, effect, cmd.ActorId, cmd.ExpiresAt);
            overrideRepo.Add(entity);
            await uow.SaveChangesAsync(ct);

            await permissionService.InvalidateCacheAsync(cmd.UserId);

            auditLogger.Log(new SecurityAuditEvent
            {
                EventType = AuditEventTypes.PermissionOverrideGranted,
                ActorId = cmd.ActorId, ActorType = "USER",
                TargetType = "User", TargetId = cmd.UserId,
                NewValues = $"{{\"permission\":\"{cmd.PermissionCode}\",\"effect\":\"{cmd.Effect}\"}}",
            });

            return ValidationResult<PermissionOverrideDto>.Ok(new PermissionOverrideDto(
                entity.OverrideId, permission.PermissionCode,
                effect.ToString(), cmd.ActorId, entity.GrantedAt, entity.ExpiresAt));
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<PermissionOverrideDto>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<PermissionOverrideDto>.Failure(ex.Message);
        }
    }
}
