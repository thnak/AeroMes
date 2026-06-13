using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Molds.Commands.RemoveMoldProduct;

public class RemoveMoldProductHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveMoldProductCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemoveMoldProductCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct);
        if (mold is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.MoldCode}' was not found.");

        mold.RemoveProductMapping(cmd.MappingId, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
