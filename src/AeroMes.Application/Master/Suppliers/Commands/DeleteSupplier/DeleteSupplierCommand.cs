using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Suppliers.Commands.DeleteSupplier;

public record DeleteSupplierCommand(string Code, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
