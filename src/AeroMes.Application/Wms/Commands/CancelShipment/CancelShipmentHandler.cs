using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CancelShipment;

public class CancelShipmentHandler(
    IShipmentOrderRepository shipmentRepo,
    IUnitOfWork uow,
    IValidator<CancelShipmentCommand> validator)
    : ICommandHandler<CancelShipmentCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        CancelShipmentCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var shipment = await shipmentRepo.GetByIdAsync(cmd.ShipmentId, ct);
            if (shipment is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy lệnh xuất hàng #{cmd.ShipmentId}.");

            shipment.Cancel(cmd.CancelledBy);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
