using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetFactoryWarehouseExports;

public record GetFactoryWarehouseExportsQuery(FactoryExportType? ExportType, FactoryExportStatus? Status)
    : IQuery<IReadOnlyList<FactoryWarehouseExportSummaryDto>>;

public record FactoryWarehouseExportSummaryDto(
    int ExportId,
    string VoucherNumber,
    FactoryExportType ExportType,
    FactoryExportStatus Status,
    int? ReferenceRequestId,
    string ReceiverOrReceivingUnit,
    int LineCount,
    DateTime CreatedAt,
    string? CreatedBy);
