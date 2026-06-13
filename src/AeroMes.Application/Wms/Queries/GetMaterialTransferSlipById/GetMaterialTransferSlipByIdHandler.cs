using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetMaterialTransferSlipById;

public class GetMaterialTransferSlipByIdHandler(IMaterialTransferSlipRepository slipRepo)
    : IQueryHandler<GetMaterialTransferSlipByIdQuery, MaterialTransferSlipDetailDto?>
{
    public async Task<MaterialTransferSlipDetailDto?> HandleAsync(
        GetMaterialTransferSlipByIdQuery query, CancellationToken ct)
    {
        var slip = await slipRepo.GetByIdWithLinesAsync(query.SlipId, ct);
        if (slip is null) return null;

        return new MaterialTransferSlipDetailDto(
            slip.SlipId,
            slip.VoucherNumber,
            slip.TransferType,
            slip.Status,
            slip.ReferenceRequestId,
            slip.SourceWarehouseId,
            slip.DestinationWarehouseId,
            slip.Notes,
            [.. slip.Lines.Select(l => new MaterialTransferLineDto(
                l.LineId,
                l.ProductCode,
                l.UnitOfMeasure,
                l.Quantity,
                l.SpecificationCode))],
            slip.CreatedAt,
            slip.CreatedBy,
            slip.UpdatedAt,
            slip.UpdatedBy);
    }
}
