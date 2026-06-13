using AeroMes.Application.Common;
using AeroMes.Domain.Traceability;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.CloseProcessRecord;

public sealed record CloseProcessRecordCommand(
    Guid ProcessRecordID,
    StepOutcome Outcome,
    string? DeviationRef,
    string? OutputLotNumber)
    : ICommand<ValidationResult<Unit>>;
