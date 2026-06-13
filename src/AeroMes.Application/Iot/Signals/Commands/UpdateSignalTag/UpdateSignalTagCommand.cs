using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Signals.Commands.UpdateSignalTag;

public record UpdateSignalTagCommand(
    string Key,
    string DisplayName,
    string Category,
    string DataType,
    string? DefaultUnit,
    decimal? TypicalMin,
    decimal? TypicalMax,
    string? Description) : ICommand<ValidationResult<Unit>>;
