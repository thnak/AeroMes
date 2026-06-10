using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.RemoveAvlItem;

public class RemoveAvlItemHandler(
    ISupplierRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveAvlItemCommand>
{
    public async Task HandleAsync(RemoveAvlItemCommand cmd, CancellationToken ct)
    {
        var supplier = await repo.GetByIdWithAvlAsync(cmd.SupplierCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.SupplierCode), cmd.SupplierCode);
        supplier.RemoveAvlItem(cmd.AvlItemId);
        await uow.SaveChangesAsync(ct);
    }
}
