using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.CreateSupplier;

public class CreateSupplierHandler(
    ISupplierRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateSupplierCommand, string>
{
    public async Task<string> HandleAsync(CreateSupplierCommand cmd, CancellationToken ct)
    {
        var supplier = Supplier.Create(
            cmd.Code, cmd.Name,
            cmd.Country, cmd.City, cmd.Address,
            cmd.Phone, cmd.Email, cmd.ContactName, cmd.TaxCode,
            cmd.CreatedBy);
        await repo.AddAsync(supplier, ct);
        await uow.SaveChangesAsync(ct);
        return supplier.SupplierCode;
    }
}
