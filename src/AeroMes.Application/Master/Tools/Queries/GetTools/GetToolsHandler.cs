using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Tools.Queries.GetTools;

public class GetToolsHandler(IToolRepository repo)
    : IQueryHandler<GetToolsQuery, IReadOnlyList<ToolDto>>
{
    public async Task<IReadOnlyList<ToolDto>> HandleAsync(GetToolsQuery query, CancellationToken ct)
    {
        var tools = await repo.GetAllAsync(
            query.ActiveOnly, query.ToolType, query.Status, query.WorkCenterId, query.Search, ct);

        return tools.Select(ToDto).ToList();
    }

    internal static ToolDto ToDto(Tool t) => new(
        t.ToolId, t.ToolCode, t.ToolName,
        t.ToolType.ToString(), t.Brand, t.Model, t.Specification,
        t.MaxUsageCount, t.CurrentUsageCount,
        UsagePercent(t.CurrentUsageCount, t.MaxUsageCount),
        t.IsReconditioningDue,
        t.Status.ToString(),
        t.CurrentWorkCenterId, t.StorageLocation,
        t.RequiresCalibration, t.NextCalibrationDue,
        t.IsActive);

    internal static decimal? UsagePercent(int currentUsage, int? maxUsage) =>
        maxUsage is int max and > 0 ? Math.Round(currentUsage * 100m / max, 1) : null;
}
