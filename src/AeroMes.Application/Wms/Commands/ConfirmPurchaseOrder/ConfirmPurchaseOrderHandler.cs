using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Wms.Commands.ConfirmPurchaseOrder;

public class ConfirmPurchaseOrderHandler(
    IPurchaseOrderRepository poRepo,
    IUnitOfWork uow)
    : ICommandHandler<ConfirmPurchaseOrderCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ConfirmPurchaseOrderCommand cmd, CancellationToken ct)
    {
        try
        {
            var po = await poRepo.GetByIdWithLinesAsync(cmd.PoId, ct);
            if (po is null) return ValidationResult<Unit>.NotFound($"PurchaseOrder '{cmd.PoId}' was not found.");

            po.Confirm(cmd.ConfirmedBy);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
