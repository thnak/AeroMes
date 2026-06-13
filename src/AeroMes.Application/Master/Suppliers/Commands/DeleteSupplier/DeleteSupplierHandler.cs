using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Suppliers.Commands.DeleteSupplier;

public class DeleteSupplierHandler(
    ISupplierRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteSupplierCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteSupplierCommand cmd, CancellationToken ct)
    {
        var supplier = await repo.GetByIdAsync(cmd.Code, ct);
        if (supplier is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.Code}' was not found.");
        supplier.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
