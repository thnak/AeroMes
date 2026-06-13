using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Energy.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Energy.Commands.CloseShiftConsumption;

public class CloseShiftConsumptionHandler(IEnergyRepository repository, IUnitOfWork uow)
    : ICommandHandler<CloseShiftConsumptionCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(CloseShiftConsumptionCommand command, CancellationToken ct)
    {
        var consumption = await repository.GetOpenShiftConsumptionAsync(
            command.MeterID, command.ShiftCode, command.ShiftDate, ct);
        if (consumption is null)
            return ValidationResult<Unit>.NotFound("Không tìm thấy bản ghi tiêu thụ ca đang mở cho đồng hồ này.");

        var endReading = await repository.GetReadingByIdAsync(command.EndReadingID, ct);
        if (endReading is null)
            return ValidationResult<Unit>.NotFound($"Số đọc #{command.EndReadingID} không tìm thấy.");

        var startReading = await repository.GetReadingByIdAsync(consumption.StartReadingID, ct);
        if (startReading is null)
            return ValidationResult<Unit>.Failure("Không tìm thấy số đọc ban đầu của ca.");

        var target = await repository.GetActiveTargetAsync(
            (await repository.GetMeterByIdAsync(command.MeterID, ct))?.MachineCode ?? "",
            command.ShiftDate, ct);

        decimal? tariffRate = null;
        consumption.Close(command.EndReadingID, endReading.ReadingValue,
            startReading.ReadingValue, tariffRate, command.QtyProduced);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
