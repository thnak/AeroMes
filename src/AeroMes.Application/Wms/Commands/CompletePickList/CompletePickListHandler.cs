using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CompletePickList;

public class CompletePickListHandler(
    IPickListRepository pickListRepo,
    IShipmentOrderRepository shipmentRepo,
    IUnitOfWork uow,
    IValidator<CompletePickListCommand> validator)
    : ICommandHandler<CompletePickListCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        CompletePickListCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var pickList = await pickListRepo.GetByIdWithLinesAsync(cmd.PickListId, ct);
            if (pickList is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy phiếu lấy hàng #{cmd.PickListId}.");

            pickList.Complete(cmd.CompletedBy);

            var shipment = await shipmentRepo.GetByIdWithLinesAsync(pickList.ShipmentId, ct);
            if (shipment is not null)
            {
                foreach (var pickLine in pickList.Lines.Where(l => l.IsConfirmed))
                {
                    var shipLine = shipment.Lines.FirstOrDefault(sl => sl.LineId == pickLine.ShipmentLineId);
                    shipLine?.RecordPicked(pickLine.PickedQty);
                }
            }

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
