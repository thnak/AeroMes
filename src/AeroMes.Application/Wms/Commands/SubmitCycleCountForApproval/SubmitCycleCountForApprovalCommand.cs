using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.SubmitCycleCountForApproval;

public record SubmitCycleCountForApprovalCommand(
    int PlanId,
    string? SubmittedBy
) : ICommand<ValidationResult<Unit>>;
