using AeroMes.Application.Common;
using AeroMes.Domain.Traceability;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.AppendLotEvent;

public sealed record AppendLotEventCommand(
    LotEventType EventType,
    string LotNumber,
    string ProductCode,
    string OperatorCode,
    DateTime EventTimestamp,
    int? WorkOrderID,
    int? RoutingStepID,
    int? LocationID,
    decimal? Quantity,
    string? UoM,
    string? Payload,
    string? EquipmentCode,
    LotEventSourceSystem SourceSystem) : ICommand<ValidationResult<Unit>>;
