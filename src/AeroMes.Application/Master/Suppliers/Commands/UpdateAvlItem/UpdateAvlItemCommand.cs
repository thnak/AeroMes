using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.UpdateAvlItem;

public record UpdateAvlItemCommand(
    string SupplierCode,
    int AvlItemId,
    AvlStatus Status,
    decimal? UnitPrice,
    string? CurrencyCode,
    int? LeadTimeDays,
    decimal? MinOrderQty,
    string? AqlLevel,
    bool IsPreferred,
    DateOnly? ApprovedFrom,
    DateOnly? ApprovedTo,
    string? Notes,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
