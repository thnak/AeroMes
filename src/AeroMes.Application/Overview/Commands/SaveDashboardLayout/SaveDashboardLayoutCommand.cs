using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Overview.Commands.SaveDashboardLayout;

public record SaveDashboardLayoutCommand(string UserId, string LayoutJson)
    : ICommand<ValidationResult<Unit>>;

public class SaveDashboardLayoutCommandHandler(IDashboardLayoutRepository repo)
    : ICommandHandler<SaveDashboardLayoutCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SaveDashboardLayoutCommand cmd, CancellationToken ct = default)
    {
        var existing = await repo.GetByUserIdAsync(cmd.UserId, ct);
        if (existing is null)
        {
            var layout = DashboardLayout.Create(cmd.UserId, cmd.LayoutJson);
            await repo.AddAsync(layout, ct);
        }
        else
        {
            existing.Update(cmd.LayoutJson);
            await repo.SaveChangesAsync(ct);
        }
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
