using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteFactoryWarehouseExport;

public record DeleteFactoryWarehouseExportCommand(int ExportId, string? DeletedBy)
    : ICommand<ValidationResult<Unit>>;
