using AeroMes.Application.Common;
using AeroMes.Domain.Energy;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Energy.Commands.RegisterMeterReading;

public record RegisterMeterReadingCommand(
    int MeterID,
    ReadingType ReadingType,
    decimal ReadingValue,
    DateTime ReadingAt,
    string? ShiftCode,
    int? WOID,
    string? EnteredBy,
    string? Notes) : ICommand<ValidationResult<long>>;
