using AeroMes.Application.Common;
using AeroMes.Domain.Traceability;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.ReleaseHold;

public sealed record ReleaseHoldCommand(
    Guid HoldID,
    HoldDispositionCode DispositionCode,
    string? DispositionNotes,
    string ReleasedBy,
    string ESignatureToken)
    : ICommand<ValidationResult<Unit>>;
