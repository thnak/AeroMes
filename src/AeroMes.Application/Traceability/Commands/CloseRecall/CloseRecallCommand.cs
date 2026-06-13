using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.CloseRecall;

public sealed record CloseRecallCommand(
    Guid RecallID,
    string ClosedBy,
    string ESignatureToken,
    string ClosureNotes)
    : ICommand<ValidationResult<Unit>>;
