using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.ApproveRegrindUsage;

public record ApproveRegrindUsageCommand(
    long BlendLogID,
    string ApprovedBy,
    string? ApprovalNotes
) : ICommand<ValidationResult<Unit>>;
