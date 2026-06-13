using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Suppliers.Commands.RemoveAvlItem;

public class RemoveAvlItemHandler(
    ISupplierRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveAvlItemCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemoveAvlItemCommand cmd, CancellationToken ct)
    {
        var supplier = await repo.GetByIdWithAvlAsync(cmd.SupplierCode, ct);
        if (supplier is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.SupplierCode}' was not found.");
        supplier.RemoveAvlItem(cmd.AvlItemId);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
