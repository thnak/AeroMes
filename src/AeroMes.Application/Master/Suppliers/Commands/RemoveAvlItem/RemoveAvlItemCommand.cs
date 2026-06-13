using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Suppliers.Commands.RemoveAvlItem;

public record RemoveAvlItemCommand(string SupplierCode, int AvlItemId, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
