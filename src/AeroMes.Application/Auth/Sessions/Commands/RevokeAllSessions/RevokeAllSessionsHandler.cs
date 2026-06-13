using AeroMes.Application.Interfaces;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.Sessions.Commands.RevokeAllSessions;

public class RevokeAllSessionsHandler(
    IUserRepository users,
    IRefreshTokenRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<RevokeAllSessionsCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RevokeAllSessionsCommand cmd, CancellationToken ct)
    {
        if (!await users.ExistsAsync(cmd.UserId, ct))
            return ValidationResult<Unit>.NotFound($"User '{cmd.UserId}' was not found.");

        var tokens = await repo.GetActiveByUserIdAsync(cmd.UserId, ct);
        foreach (var t in tokens) t.Revoke();
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
