using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreateDisassemblyOrder;

public record CreateDisassemblyOrderCommand(
    DisassemblyOrderType OrderType,
    string SourceProductCode,
    decimal SourceQty,
    int? PurchaseOrderID = null,
    DateTime? Deadline = null,
    string? Notes = null) : ICommand<ValidationResult<int>>;
