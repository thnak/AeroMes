using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.DeleteSupplier;

public class DeleteSupplierHandler(
    ISupplierRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteSupplierCommand>
{
    public async Task HandleAsync(DeleteSupplierCommand cmd, CancellationToken ct)
    {
        var supplier = await repo.GetByIdAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.Code), cmd.Code);
        supplier.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
