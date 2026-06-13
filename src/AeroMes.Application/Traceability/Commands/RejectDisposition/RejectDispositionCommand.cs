using AeroMes.Application.Common;
using AeroMes.Domain.Traceability;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.RejectDisposition;

public sealed record RejectDispositionCommand(
    Guid HoldID,
    HoldDispositionCode DispositionCode,
    string DispositionNotes,
    string ReleasedBy,
    string ESignatureToken)
    : ICommand<ValidationResult<Unit>>;
