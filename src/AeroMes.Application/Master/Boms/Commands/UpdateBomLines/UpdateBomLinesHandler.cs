using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.UpdateBomLines;

public class UpdateBomLinesHandler(
    IBomHeaderRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateBomLinesCommand> validator) : ICommandHandler<UpdateBomLinesCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateBomLinesCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var header = await repo.GetByProductAndVersionAsync(cmd.ProductCode, cmd.Version, ct)
                ?? throw new EntityNotFoundException(nameof(BomHeader), $"{cmd.ProductCode}/{cmd.Version}");

            header.ReplaceLines(
                cmd.Lines
                    .Select(l => (l.LineNo, l.ComponentCode, l.RequiredQty, l.UoMCode,
                        l.ScrapFactor, l.IsPhantom, l.Notes))
                    .ToList(),
                cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<Unit>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
