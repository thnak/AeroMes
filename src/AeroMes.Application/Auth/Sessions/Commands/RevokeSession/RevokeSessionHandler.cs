using AeroMes.Application.Interfaces;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.Sessions.Commands.RevokeSession;

public class RevokeSessionHandler(
    IRefreshTokenRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<RevokeSessionCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RevokeSessionCommand cmd, CancellationToken ct)
    {
        var token = await repo.GetActiveByTokenIdAndUserAsync(cmd.TokenId, cmd.UserId, ct);
        if (token is null) return ValidationResult<Unit>.NotFound($"Session '{cmd.TokenId}' was not found.");

        token.Revoke();
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
