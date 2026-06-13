using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Boms.Commands.ApproveBom;

public class ApproveBomHandler(
    IBomHeaderRepository repo,
    IUnitOfWork uow) : ICommandHandler<ApproveBomCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ApproveBomCommand cmd, CancellationToken ct)
    {
        var header = await repo.GetByProductAndVersionAsync(cmd.ProductCode, cmd.Version, ct);
        if (header is null) return ValidationResult<Unit>.NotFound($"Entity '{$"{cmd.ProductCode}/{cmd.Version}"}' was not found.");

        header.Approve(cmd.ApprovedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
