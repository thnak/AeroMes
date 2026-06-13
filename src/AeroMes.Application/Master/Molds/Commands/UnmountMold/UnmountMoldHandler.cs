using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Molds.Commands.UnmountMold;

public class UnmountMoldHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<UnmountMoldCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UnmountMoldCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct);
        if (mold is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.MoldCode}' was not found.");

        mold.Unmount(cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
