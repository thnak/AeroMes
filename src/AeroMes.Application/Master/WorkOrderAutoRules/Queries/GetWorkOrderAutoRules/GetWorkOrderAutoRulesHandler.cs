using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.WorkOrderAutoRules.Queries.GetWorkOrderAutoRules;

public class GetWorkOrderAutoRulesHandler(IWorkOrderAutoRulesRepository repo)
    : IQueryHandler<GetWorkOrderAutoRulesQuery, IReadOnlyList<WorkOrderAutoRulesDto>>
{
    public async Task<IReadOnlyList<WorkOrderAutoRulesDto>> HandleAsync(GetWorkOrderAutoRulesQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(ct);
        return items.Select(x => new WorkOrderAutoRulesDto(
            x.RuleId, x.WorkCenterId, x.AutoStartEnabled,
            x.AutoCompleteOnTargetReached, x.RequireDeleteConfirmToken,
            x.MaxConcurrentJobs, x.RequireCertification)).ToList();
    }
}
