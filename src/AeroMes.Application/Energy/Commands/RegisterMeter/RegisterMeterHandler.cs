using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Energy;
using AeroMes.Domain.Energy.Repositories;
using AeroMes.Domain.Exceptions;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Energy.Commands.RegisterMeter;

public class RegisterMeterHandler(
    IEnergyRepository repository,
    IUnitOfWork uow,
    IValidator<RegisterMeterCommand> validator)
    : ICommandHandler<RegisterMeterCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(RegisterMeterCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        if (await repository.MeterCodeExistsAsync(command.MeterCode, ct))
            return ValidationResult<int>.Failure($"Mã đồng hồ '{command.MeterCode}' đã tồn tại.");

        try
        {
            var meter = Meter.Create(
                command.MeterCode, command.MeterName, command.UtilityType, command.Unit,
                command.MachineCode, command.WorkCenterID, command.IsSubMeter, command.ParentMeterID,
                command.TariffID, command.OpcUaNodeId, command.CreatedBy);
            await repository.AddMeterAsync(meter, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(meter.MeterID);
        }
        catch (DomainException ex) { return ValidationResult<int>.Failure(ex.Message); }
    }
}
