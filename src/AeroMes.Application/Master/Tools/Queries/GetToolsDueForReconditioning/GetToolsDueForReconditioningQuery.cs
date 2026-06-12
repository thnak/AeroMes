using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Tools.Queries.GetToolsDueForReconditioning;

public record GetToolsDueForReconditioningQuery : IQuery<IReadOnlyList<ToolReconditioningDueDto>>;

public record ToolReconditioningDueDto(
    int ToolId,
    string ToolCode,
    string ToolName,
    string Status,
    int CurrentUsageCount,
    int UsageCountAtLastPm,
    int PmIntervalCount,
    int UsageSinceLastPm,
    bool IsOverdue);
