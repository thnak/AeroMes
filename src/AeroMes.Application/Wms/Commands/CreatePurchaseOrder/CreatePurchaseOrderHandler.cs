using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderHandler(
    IPurchaseOrderRepository poRepo,
    ISupplierRepository supplierRepo,
    IUnitOfWork uow,
    IValidator<CreatePurchaseOrderCommand> validator)
    : ICommandHandler<CreatePurchaseOrderCommand, ValidationResult<PoCreatedResult>>
{
    public async Task<ValidationResult<PoCreatedResult>> HandleAsync(CreatePurchaseOrderCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<PoCreatedResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            if (!await supplierRepo.CodeExistsAsync(cmd.SupplierCode, ct))
                throw new EntityNotFoundException("Supplier", cmd.SupplierCode);

            var po = PurchaseOrder.Create(cmd.PoCode, cmd.SupplierCode, cmd.ExpectedDeliveryDate, cmd.Notes, cmd.CreatedBy);
            foreach (var l in cmd.Lines)
                po.AddLine(l.ProductCode, l.OrderedQty, l.UnitPrice, l.ExpectedLotNumber);

            await poRepo.AddAsync(po, ct);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<PoCreatedResult>.Ok(new PoCreatedResult(po.PoId, po.PoCode));
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<PoCreatedResult>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<PoCreatedResult>.Failure(ex.Message);
        }
    }
}
