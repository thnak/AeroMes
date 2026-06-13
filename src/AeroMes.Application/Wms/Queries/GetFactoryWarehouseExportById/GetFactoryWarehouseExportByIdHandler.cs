using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetFactoryWarehouseExportById;

public class GetFactoryWarehouseExportByIdHandler(IFactoryWarehouseExportRepository exportRepo)
    : IQueryHandler<GetFactoryWarehouseExportByIdQuery, FactoryWarehouseExportDetailDto?>
{
    public async Task<FactoryWarehouseExportDetailDto?> HandleAsync(
        GetFactoryWarehouseExportByIdQuery query, CancellationToken ct)
    {
        var export = await exportRepo.GetByIdWithLinesAsync(query.ExportId, ct);
        if (export is null) return null;

        return new FactoryWarehouseExportDetailDto(
            export.ExportId,
            export.VoucherNumber,
            export.ExportType,
            export.Status,
            export.ReferenceRequestId,
            export.ReceiverOrReceivingUnit,
            export.Notes,
            [.. export.Lines.Select(l => new FactoryExportLineDto(
                l.LineId,
                l.ProductCode,
                l.UnitOfMeasure,
                l.Quantity,
                l.SourceWarehouseId,
                l.SpecificationCode))],
            export.CreatedAt,
            export.CreatedBy,
            export.UpdatedAt,
            export.UpdatedBy);
    }
}
