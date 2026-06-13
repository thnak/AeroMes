using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.AddCharacteristic;

public class AddCharacteristicHandler(
    IInspectionPlanRepository repo,
    IUnitOfWork uow,
    IValidator<AddCharacteristicCommand> validator) : ICommandHandler<AddCharacteristicCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddCharacteristicCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        var plan = await repo.GetByIdWithCharacteristicsAsync(cmd.PlanId, ct);
        if (plan is null)
            return ValidationResult<int>.NotFound($"Inspection plan {cmd.PlanId} not found.");

        try
        {
            var characteristic = InspectionCharacteristic.Create(
                cmd.PlanId, cmd.Sequence, cmd.CharName, cmd.MeasurementType,
                cmd.SpecMin, cmd.SpecMax, cmd.SpecNominal, cmd.Unit,
                cmd.AttributeSpec, cmd.IsRequired, cmd.SeverityLevel,
                cmd.DefectCodeLink, cmd.Notes);

            plan.AddCharacteristic(characteristic);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(characteristic.CharId);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
