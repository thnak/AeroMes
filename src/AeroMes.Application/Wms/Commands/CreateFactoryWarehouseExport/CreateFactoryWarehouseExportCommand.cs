using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateFactoryWarehouseExport;

public record CreateFactoryWarehouseExportCommand(
    FactoryExportType ExportType,
    int? ReferenceRequestId,
    string ReceiverOrReceivingUnit,
    string? Notes,
    IReadOnlyList<ExportLineInput> Lines,
    string? CreatedBy
) : ICommand<ValidationResult<FactoryWarehouseExportCreatedResult>>;

public record ExportLineInput(
    string ProductCode,
    string UnitOfMeasure,
    decimal Quantity,
    int SourceWarehouseId,
    string? SpecificationCode);

public record FactoryWarehouseExportCreatedResult(int ExportId, string VoucherNumber);
