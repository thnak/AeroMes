using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Commands.UpdateDisassemblyBom;

public class UpdateDisassemblyBomHandler(
    IDisassemblyBomRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateDisassemblyBomCommand> validator)
    : ICommandHandler<UpdateDisassemblyBomCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateDisassemblyBomCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdWithLinesAsync(cmd.DisassemblyBomId, ct);
            if (entity is null)
                return ValidationResult<Unit>.NotFound(
                    $"DisassemblyBom '{cmd.DisassemblyBomId}' không tìm thấy.");

            entity.Update(cmd.BomName, cmd.LossRatio, cmd.EffectiveDate, cmd.ExpiryDate, cmd.UpdatedBy);

            entity.ReplaceLines(
                cmd.Lines.Select(l => (l.LineNo, l.ComponentCode, l.ComponentType,
                    l.RecoveryRate, l.FixedQuantity, l.UoMCode, l.Notes)).ToList(),
                cmd.UpdatedBy);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
