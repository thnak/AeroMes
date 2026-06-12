using AeroMes.Domain.Master;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Tools.Queries.GetTools;

public record GetToolsQuery(
    bool ActiveOnly = true,
    ToolType? ToolType = null,
    ToolStatus? Status = null,
    int? WorkCenterId = null,
    string? Search = null) : IQuery<IReadOnlyList<ToolDto>>;

public record ToolDto(
    int ToolId,
    string ToolCode,
    string ToolName,
    string ToolType,
    string? Brand,
    string? Model,
    string? Specification,
    int? MaxUsageCount,
    int CurrentUsageCount,
    decimal? UsagePercent,
    bool ReconditioningDue,
    string Status,
    int? CurrentWorkCenterId,
    string? StorageLocation,
    bool RequiresCalibration,
    DateOnly? NextCalibrationDue,
    bool IsActive);
