using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Overview.Commands.DeleteDashboardLayout;

public record DeleteDashboardLayoutCommand(string UserId) : ICommand<ValidationResult<Unit>>;

public class DeleteDashboardLayoutCommandHandler(IDashboardLayoutRepository repo)
    : ICommandHandler<DeleteDashboardLayoutCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteDashboardLayoutCommand cmd, CancellationToken ct = default)
    {
        var existing = await repo.GetByUserIdAsync(cmd.UserId, ct);
        if (existing is null) return ValidationResult<Unit>.NotFound("Dashboard layout not found.");
        await repo.DeleteAsync(existing, ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
