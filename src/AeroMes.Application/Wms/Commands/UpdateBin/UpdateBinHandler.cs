using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateBin;

public class UpdateBinHandler(
    IBinRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateBinCommand> validator)
    : ICommandHandler<UpdateBinCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateBinCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var entity = await repo.GetByIdAsync(cmd.BinId, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"Ô '{cmd.BinId}' không tồn tại.");

        entity.UpdateDetails(cmd.BinLevel, cmd.BinType, cmd.MaxQty, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
