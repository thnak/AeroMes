using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.StageHandovers.Queries.GetHandoverFormDetail;

public sealed class GetHandoverFormDetailHandler(IStageHandoverRepository repo)
    : IQueryHandler<GetHandoverFormDetailQuery, HandoverFormDetailDto?>
{
    public async Task<HandoverFormDetailDto?> HandleAsync(GetHandoverFormDetailQuery query, CancellationToken ct)
    {
        var form = await repo.GetByIdWithLinesAsync(query.FormId, ct);
        if (form is null) return null;

        return new HandoverFormDetailDto(
            form.FormID, form.FormNumber, form.FormType.ToString(), form.Status.ToString(),
            form.FromWorkOrderID, form.FromRoutingStepID,
            form.ToWorkOrderID, form.ToRoutingStepID,
            form.HandoverDate, form.Notes,
            form.CreatedBy, form.CreatedAt,
            form.Lines.Select(l => new HandoverLineDto(l.LineID, l.ProductCode, l.Qty, l.Unit, l.Notes)).ToList());
    }
}
