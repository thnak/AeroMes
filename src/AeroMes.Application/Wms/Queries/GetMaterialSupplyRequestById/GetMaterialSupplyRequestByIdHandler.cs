using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetMaterialSupplyRequestById;

public class GetMaterialSupplyRequestByIdHandler(IMaterialSupplyRequestRepository repo)
    : IQueryHandler<GetMaterialSupplyRequestByIdQuery, MaterialSupplyRequestDetailDto?>
{
    public async Task<MaterialSupplyRequestDetailDto?> HandleAsync(
        GetMaterialSupplyRequestByIdQuery query, CancellationToken ct)
    {
        var request = await repo.GetByIdWithLinesAsync(query.RequestId, ct);
        if (request is null) return null;

        return new MaterialSupplyRequestDetailDto(
            request.RequestId,
            request.VoucherNumber,
            request.RequestType,
            request.Status,
            request.RequesterUnit,
            request.RequiredByDate,
            request.Notes,
            [.. request.Lines.Select(l => new MaterialSupplyRequestLineDto(
                l.LineId,
                l.ProductCode,
                l.UnitOfMeasure,
                l.RequestedQuantity,
                l.WarehouseId,
                l.Notes))],
            request.CreatedAt,
            request.CreatedBy,
            request.UpdatedAt,
            request.UpdatedBy);
    }
}
