using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.UpdateCharacteristic;

public class UpdateCharacteristicHandler(
    IInspectionCharacteristicRepository charRepo,
    IUnitOfWork uow,
    IValidator<UpdateCharacteristicCommand> validator) : ICommandHandler<UpdateCharacteristicCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateCharacteristicCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var characteristic = await charRepo.GetByIdAsync(cmd.CharId, ct);
        if (characteristic is null)
            return ValidationResult<Unit>.NotFound($"Characteristic {cmd.CharId} not found.");

        try
        {
            characteristic.Update(
                cmd.Sequence, cmd.CharName, cmd.MeasurementType,
                cmd.SpecMin, cmd.SpecMax, cmd.SpecNominal, cmd.Unit,
                cmd.AttributeSpec, cmd.IsRequired, cmd.SeverityLevel,
                cmd.DefectCodeLink, cmd.Notes);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
