using AeroMes.Application.Common;
using AeroMes.Domain.Traceability;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.PlaceHold;

public sealed record PlaceHoldCommand(
    string LotNumber,
    HoldReason HoldReason,
    string InitiatedBy,
    string? ProductCode,
    int? WorkOrderID,
    string? HoldDescription,
    string? HoldReference)
    : ICommand<ValidationResult<Guid>>;
