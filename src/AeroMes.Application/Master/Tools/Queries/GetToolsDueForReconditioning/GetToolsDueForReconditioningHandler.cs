using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Tools.Queries.GetToolsDueForReconditioning;

public class GetToolsDueForReconditioningHandler(IToolRepository repo)
    : IQueryHandler<GetToolsDueForReconditioningQuery, IReadOnlyList<ToolReconditioningDueDto>>
{
    public async Task<IReadOnlyList<ToolReconditioningDueDto>> HandleAsync(
        GetToolsDueForReconditioningQuery query, CancellationToken ct)
    {
        // 0.9 — surfaces tools approaching the reconditioning interval, not only overdue ones.
        var tools = await repo.GetDueForReconditioningAsync(0.9, ct);

        return tools
            .Select(t => new ToolReconditioningDueDto(
                t.ToolId, t.ToolCode, t.ToolName, t.Status.ToString(),
                t.CurrentUsageCount, t.UsageCountAtLastPm, t.PmIntervalCount!.Value,
                t.CurrentUsageCount - t.UsageCountAtLastPm,
                t.IsReconditioningDue))
            .ToList();
    }
}
