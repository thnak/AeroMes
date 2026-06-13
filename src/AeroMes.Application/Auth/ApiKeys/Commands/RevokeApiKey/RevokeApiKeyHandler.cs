using AeroMes.Application.Auth;
using AeroMes.Application.Interfaces;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.ApiKeys.Commands.RevokeApiKey;

public class RevokeApiKeyHandler(IApiKeyRepository repo, IUnitOfWork uow, IAuditLogger audit)
    : ICommandHandler<RevokeApiKeyCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RevokeApiKeyCommand cmd, CancellationToken ct)
    {
        var key = await repo.GetByIdAsync(cmd.ApiKeyId, ct);
        if (key is null) return ValidationResult<Unit>.NotFound($"ApiKey '{cmd.ApiKeyId}' was not found.");

        key.Revoke();
        await uow.SaveChangesAsync(ct);

        audit.Log(new SecurityAuditEvent
        {
            EventType = "ApiKey:Revoke",
            ActorId = cmd.ActorId,
            TargetType = "ApiKey",
            TargetId = cmd.ApiKeyId.ToString(),
            Outcome = "Success",
        });
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
