using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.StageHandovers.Queries.GetHandoverForms;

public sealed class GetHandoverFormsHandler(IStageHandoverRepository repo)
    : IQueryHandler<GetHandoverFormsQuery, IReadOnlyList<HandoverFormSummaryDto>>
{
    public async Task<IReadOnlyList<HandoverFormSummaryDto>> HandleAsync(GetHandoverFormsQuery query, CancellationToken ct)
    {
        IReadOnlyList<Domain.Production.StageHandoverForm> forms;

        if (query.WorkOrderId.HasValue)
            forms = await repo.GetByWorkOrderAsync(query.WorkOrderId.Value, ct);
        else
            forms = await repo.GetByDateRangeAsync(
                query.From ?? DateTime.UtcNow.AddDays(-30),
                query.To ?? DateTime.UtcNow.AddDays(1), ct);

        return forms.Select(f => new HandoverFormSummaryDto(
            f.FormID, f.FormNumber, f.FormType.ToString(), f.Status.ToString(),
            f.FromWorkOrderID, f.FromRoutingStepID,
            f.ToWorkOrderID, f.ToRoutingStepID,
            f.HandoverDate, f.Lines.Count,
            f.CreatedBy, f.CreatedAt)).ToList();
    }
}
