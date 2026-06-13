using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Energy;
using AeroMes.Domain.Energy.Repositories;
using AeroMes.Domain.Exceptions;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Energy.Commands.RegisterMeterReading;

public class RegisterMeterReadingHandler(
    IEnergyRepository repository,
    IUnitOfWork uow,
    IValidator<RegisterMeterReadingCommand> validator)
    : ICommandHandler<RegisterMeterReadingCommand, ValidationResult<long>>
{
    public async Task<ValidationResult<long>> HandleAsync(RegisterMeterReadingCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<long>.Invalid(v.ToErrorDictionary());

        try
        {
            var reading = MeterReading.Create(
                command.MeterID, command.ReadingType, command.ReadingValue,
                command.ReadingAt, command.ShiftCode, command.EnteredBy, command.Notes);
            await repository.AddReadingAsync(reading, ct);

            if (command.ReadingType == ReadingType.ShiftStart && command.ShiftCode != null)
            {
                var date = DateOnly.FromDateTime(command.ReadingAt);
                var consumption = ShiftConsumption.OpenShift(
                    command.MeterID, command.ShiftCode, date, reading.ReadingID, command.WOID);
                await repository.AddConsumptionAsync(consumption, ct);
            }

            await uow.SaveChangesAsync(ct);
            return ValidationResult<long>.Ok(reading.ReadingID);
        }
        catch (DomainException ex) { return ValidationResult<long>.Failure(ex.Message); }
    }
}
