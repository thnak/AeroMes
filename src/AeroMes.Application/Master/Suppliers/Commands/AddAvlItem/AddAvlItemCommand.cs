using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.AddAvlItem;

public record AddAvlItemCommand(
    string SupplierCode,
    string ProductCode,
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
    string? CreatedBy) : ICommand<ValidationResult<int>>;
