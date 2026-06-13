using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Energy.Commands.CloseShiftConsumption;

public record CloseShiftConsumptionCommand(
    int MeterID,
    string ShiftCode,
    DateOnly ShiftDate,
    long EndReadingID,
    int? QtyProduced) : ICommand<ValidationResult<Unit>>;
