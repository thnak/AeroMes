using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierHandler(
    ISupplierRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateSupplierCommand>
{
    public async Task HandleAsync(UpdateSupplierCommand cmd, CancellationToken ct)
    {
        var supplier = await repo.GetByIdAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.Code), cmd.Code);
        supplier.UpdateDetails(
            cmd.Name, cmd.Country, cmd.City, cmd.Address,
            cmd.Phone, cmd.Email, cmd.ContactName, cmd.TaxCode,
            cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
