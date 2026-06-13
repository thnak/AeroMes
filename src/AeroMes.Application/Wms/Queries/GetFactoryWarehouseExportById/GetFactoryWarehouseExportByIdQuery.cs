using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetFactoryWarehouseExportById;

public record GetFactoryWarehouseExportByIdQuery(int ExportId)
    : IQuery<FactoryWarehouseExportDetailDto?>;

public record FactoryWarehouseExportDetailDto(
    int ExportId,
    string VoucherNumber,
    FactoryExportType ExportType,
    FactoryExportStatus Status,
    int? ReferenceRequestId,
    string ReceiverOrReceivingUnit,
    string? Notes,
    IReadOnlyList<FactoryExportLineDto> Lines,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy);

public record FactoryExportLineDto(
    int LineId,
    string ProductCode,
    string UnitOfMeasure,
    decimal Quantity,
    int SourceWarehouseId,
    string? SpecificationCode);
