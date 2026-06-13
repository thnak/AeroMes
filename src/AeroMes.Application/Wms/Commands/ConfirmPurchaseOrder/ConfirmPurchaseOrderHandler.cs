using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

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
            var po = await poRepo.GetByIdWithLinesAsync(cmd.PoId, ct)
                ?? throw new EntityNotFoundException("PurchaseOrder", cmd.PoId);

            po.Confirm(cmd.ConfirmedBy);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<Unit>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
