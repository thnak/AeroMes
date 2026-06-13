using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.OpenProcessRecord;

public sealed record OpenProcessRecordCommand(
    string LotNumber,
    string ProductCode,
    int WorkOrderID,
    long? JobID,
    int RoutingStepID,
    int StepSequence,
    string StepName,
    string OperatorCode,
    string? MachineCode,
    string? BOMRevision,
    string? RoutingRevision,
    string? ControlPlanRev,
    string? CertificationRef,
    string? CalibrationRef)
    : ICommand<ValidationResult<Guid>>;
