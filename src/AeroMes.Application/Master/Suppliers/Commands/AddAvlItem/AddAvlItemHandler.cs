using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.AddAvlItem;

public class AddAvlItemHandler(
    ISupplierRepository repo,
    IUnitOfWork uow) : ICommandHandler<AddAvlItemCommand, int>
{
    public async Task<int> HandleAsync(AddAvlItemCommand cmd, CancellationToken ct)
    {
        var supplier = await repo.GetByIdWithAvlAsync(cmd.SupplierCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.SupplierCode), cmd.SupplierCode);
        var item = supplier.AddAvlItem(
            cmd.ProductCode, cmd.Status,
            cmd.UnitPrice, cmd.CurrencyCode, cmd.LeadTimeDays,
            cmd.MinOrderQty, cmd.AqlLevel, cmd.IsPreferred,
            cmd.ApprovedFrom, cmd.ApprovedTo, cmd.Notes);
        await uow.SaveChangesAsync(ct);
        return item.AvlItemId;
    }
}
