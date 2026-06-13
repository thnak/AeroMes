using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteFactoryWarehouseReceipt;

public class DeleteFactoryWarehouseReceiptHandler(
    IFactoryWarehouseReceiptRepository receiptRepo,
    IUnitOfWork uow)
    : ICommandHandler<DeleteFactoryWarehouseReceiptCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteFactoryWarehouseReceiptCommand cmd, CancellationToken ct)
    {
        var receipt = await receiptRepo.GetByIdAsync(cmd.ReceiptId, ct);
        if (receipt is null)
            return ValidationResult<Unit>.NotFound($"Phiếu nhập '{cmd.ReceiptId}' không tồn tại.");

        try
        {
            receipt.SoftDelete(cmd.DeletedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
