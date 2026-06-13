using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ApproveCycleCount;

public record ApproveCycleCountCommand(
    int PlanId,
    string? ApprovedBy,
    decimal VarianceThresholdPct,
    string? Notes
) : ICommand<ValidationResult<ApproveCycleCountResult>>;

public record ApproveCycleCountResult(int ApprovedLines, int RejectedForRecountLines);
