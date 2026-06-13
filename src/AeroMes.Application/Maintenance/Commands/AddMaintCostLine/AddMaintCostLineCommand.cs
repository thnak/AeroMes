using AeroMes.Application.Common;
using AeroMes.Domain.Maintenance;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Maintenance.Commands.AddMaintCostLine;

public record AddMaintCostLineCommand(
    int MaintOrderID,
    CostCategory CostCategory,
    string? ProductCode,
    string? LotNumber,
    decimal? QtyUsed,
    decimal? UnitCost,
    string? OperatorID,
    decimal? LaborHours,
    decimal? LaborRateSnapshot,
    int? SupplierID,
    string? InvoiceRef,
    decimal? InvoiceAmount,
    string PostedBy) : ICommand<ValidationResult<Unit>>;
