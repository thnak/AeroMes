using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.SealCarton;

public class SealCartonHandler(
    ICartonRepository cartonRepo,
    IUnitOfWork uow,
    IValidator<SealCartonCommand> validator)
    : ICommandHandler<SealCartonCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SealCartonCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var carton = await cartonRepo.GetByIdWithContentsAsync(cmd.CartonId, ct);
            if (carton is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy thùng #{cmd.CartonId}.");

            carton.Seal(cmd.GrossWeightKg, cmd.SealedBy);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
