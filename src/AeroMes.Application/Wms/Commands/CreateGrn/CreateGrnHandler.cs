using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Wms.Commands.CreateGrn;

public class CreateGrnHandler(
    IGoodsReceiptNoteRepository grnRepo,
    IPurchaseOrderRepository poRepo,
    IUnitOfWork uow,
    IValidator<CreateGrnCommand> validator)
    : ICommandHandler<CreateGrnCommand, ValidationResult<GrnCreatedResult>>
{
    public async Task<ValidationResult<GrnCreatedResult>> HandleAsync(CreateGrnCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<GrnCreatedResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            if (cmd.PoId.HasValue)
            {
                var po = await poRepo.GetByIdAsync(cmd.PoId.Value, ct);
                if (po is null) return ValidationResult<GrnCreatedResult>.NotFound($"PurchaseOrder '{cmd.PoId.Value}' was not found.");
                if (po.Status is not (PoStatus.Confirmed or PoStatus.PartiallyReceived))
                    throw new DomainException(
                        $"PO '{po.PoCode}' must be Confirmed or PartiallyReceived to receive against. Current: {po.Status}.");
            }

            var grn = GoodsReceiptNote.Create(
                cmd.GrnCode, cmd.PoId, cmd.StorageLocationId,
                cmd.ReceivedBy, cmd.ReceivedAt, cmd.DeliveryNoteRef, cmd.Notes, cmd.CreatedBy);

            await grnRepo.AddAsync(grn, ct);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<GrnCreatedResult>.Ok(new GrnCreatedResult(grn.GrnId, grn.GrnCode));
        }        catch (DomainException ex)
        {
            return ValidationResult<GrnCreatedResult>.Failure(ex.Message);
        }
    }
}
