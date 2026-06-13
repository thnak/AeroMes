using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Molds.Commands.ScrapMold;

public class ScrapMoldHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<ScrapMoldCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ScrapMoldCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct);
        if (mold is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.MoldCode}' was not found.");

        mold.Scrap(cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
