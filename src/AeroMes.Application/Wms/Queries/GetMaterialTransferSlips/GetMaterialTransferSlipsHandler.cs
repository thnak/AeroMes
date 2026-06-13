using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetMaterialTransferSlips;

public class GetMaterialTransferSlipsHandler(IMaterialTransferSlipRepository slipRepo)
    : IQueryHandler<GetMaterialTransferSlipsQuery, IReadOnlyList<MaterialTransferSlipSummaryDto>>
{
    public async Task<IReadOnlyList<MaterialTransferSlipSummaryDto>> HandleAsync(
        GetMaterialTransferSlipsQuery query, CancellationToken ct)
    {
        var slips = await slipRepo.GetAllAsync(query.TransferType, query.Status, ct);

        return [.. slips.Select(s => new MaterialTransferSlipSummaryDto(
            s.SlipId,
            s.VoucherNumber,
            s.TransferType,
            s.Status,
            s.ReferenceRequestId,
            s.SourceWarehouseId,
            s.DestinationWarehouseId,
            s.Lines.Count,
            s.CreatedAt,
            s.CreatedBy))];
    }
}
