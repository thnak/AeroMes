using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.DeleteSupplier;

public record DeleteSupplierCommand(string Code, string? DeletedBy) : ICommand;
