using AeroMes.Application.Common;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.BulkHoldFromForwardTrace;

public sealed record BulkHoldFromForwardTraceCommand(
    string SuspectLotNumber,
    HoldReason HoldReason,
    string HoldReference,
    string InitiatedBy,
    string? HoldDescription,
    int MaxDepth = 20)
    : ICommand<ValidationResult<BulkHoldResultDto>>;
