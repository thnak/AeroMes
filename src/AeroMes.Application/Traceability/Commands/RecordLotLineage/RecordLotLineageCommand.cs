using AeroMes.Application.Common;
using AeroMes.Domain.Traceability;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.RecordLotLineage;

public sealed record RecordLotLineageCommand(
    string ParentLotNumber,
    string ChildLotNumber,
    LineageType LineageType,
    int? WorkOrderID,
    int? RoutingStepID,
    decimal? QuantityConsumed,
    string? UoM) : ICommand<ValidationResult<Unit>>;
