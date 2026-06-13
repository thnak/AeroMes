using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetFinishedProductIntakeRequestById;

public class GetFinishedProductIntakeRequestByIdHandler(IFinishedProductIntakeRequestRepository repo)
    : IQueryHandler<GetFinishedProductIntakeRequestByIdQuery, FinishedProductIntakeRequestDetailDto?>
{
    public async Task<FinishedProductIntakeRequestDetailDto?> HandleAsync(
        GetFinishedProductIntakeRequestByIdQuery query, CancellationToken ct)
    {
        var request = await repo.GetByIdWithLinesAsync(query.IntakeRequestId, ct);
        if (request is null) return null;

        return new FinishedProductIntakeRequestDetailDto(
            request.IntakeRequestId,
            request.RequestNumber,
            request.IntakePurpose,
            request.WarehouseType,
            request.Status,
            request.ProductionOrderId,
            request.RequesterUnit,
            request.RequestDate,
            request.SentAt,
            request.Notes,
            [.. request.Lines.Select(l => new IntakeRequestLineDto(
                l.LineId,
                l.ProductCode,
                l.UnitOfMeasure,
                l.RequestedQuantity,
                l.WarehouseId,
                l.IsDefective,
                l.DefectReason,
                l.ActualReceivedQuantity,
                l.Notes))],
            request.CreatedAt,
            request.CreatedBy,
            request.UpdatedAt,
            request.UpdatedBy);
    }
}
