using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.UpdateAvlItem;

public class UpdateAvlItemHandler(
    ISupplierRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateAvlItemCommand>
{
    public async Task HandleAsync(UpdateAvlItemCommand cmd, CancellationToken ct)
    {
        var supplier = await repo.GetByIdWithAvlAsync(cmd.SupplierCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.SupplierCode), cmd.SupplierCode);
        supplier.UpdateAvlItem(
            cmd.AvlItemId, cmd.Status,
            cmd.UnitPrice, cmd.CurrencyCode, cmd.LeadTimeDays,
            cmd.MinOrderQty, cmd.AqlLevel, cmd.IsPreferred,
            cmd.ApprovedFrom, cmd.ApprovedTo, cmd.Notes);
        await uow.SaveChangesAsync(ct);
    }
}
