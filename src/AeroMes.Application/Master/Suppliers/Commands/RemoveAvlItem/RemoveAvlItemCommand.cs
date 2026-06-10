using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.RemoveAvlItem;

public record RemoveAvlItemCommand(string SupplierCode, int AvlItemId, string? DeletedBy) : ICommand;
