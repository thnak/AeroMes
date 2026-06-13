using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.SetBomDefault;

public class SetBomDefaultHandler(
    IBomHeaderRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<SetBomDefaultCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(SetBomDefaultCommand cmd, CancellationToken ct)
    {
        try
        {
            var header = await repo.GetByProductAndVersionAsync(cmd.ProductCode, cmd.Version, ct);
            if (header is null)
                return ValidationResult<Unit>.NotFound($"BOM '{cmd.ProductCode}/{cmd.Version}' không tìm thấy.");

            // Clear the previous default of the same BomType for this product
            var previousDefault = await repo.GetDefaultByProductAndTypeAsync(cmd.ProductCode, header.BomType, ct);
            if (previousDefault is not null && previousDefault.BomHeaderId != header.BomHeaderId)
                previousDefault.ClearDefault(cmd.UpdatedBy);

            header.SetAsDefault(cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
