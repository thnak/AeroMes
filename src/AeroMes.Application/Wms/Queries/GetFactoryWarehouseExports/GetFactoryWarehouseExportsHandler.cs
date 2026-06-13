using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetFactoryWarehouseExports;

public class GetFactoryWarehouseExportsHandler(IFactoryWarehouseExportRepository exportRepo)
    : IQueryHandler<GetFactoryWarehouseExportsQuery, IReadOnlyList<FactoryWarehouseExportSummaryDto>>
{
    public async Task<IReadOnlyList<FactoryWarehouseExportSummaryDto>> HandleAsync(
        GetFactoryWarehouseExportsQuery query, CancellationToken ct)
    {
        var exports = await exportRepo.GetAllAsync(query.ExportType, query.Status, ct);

        return [.. exports.Select(e => new FactoryWarehouseExportSummaryDto(
            e.ExportId,
            e.VoucherNumber,
            e.ExportType,
            e.Status,
            e.ReferenceRequestId,
            e.ReceiverOrReceivingUnit,
            e.Lines.Count,
            e.CreatedAt,
            e.CreatedBy))];
    }
}
