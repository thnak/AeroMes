using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateFactoryWarehouseExport;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateFactoryWarehouseExport;

public record UpdateFactoryWarehouseExportCommand(
    int ExportId,
    FactoryExportType ExportType,
    int? ReferenceRequestId,
    string ReceiverOrReceivingUnit,
    string? Notes,
    IReadOnlyList<ExportLineInput> Lines,
    string? UpdatedBy
) : ICommand<ValidationResult<Unit>>;
