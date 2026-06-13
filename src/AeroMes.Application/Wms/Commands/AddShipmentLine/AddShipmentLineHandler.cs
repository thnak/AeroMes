using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.AddShipmentLine;

public class AddShipmentLineHandler(
    IShipmentOrderRepository shipmentRepo,
    IUnitOfWork uow,
    IValidator<AddShipmentLineCommand> validator)
    : ICommandHandler<AddShipmentLineCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        AddShipmentLineCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var shipment = await shipmentRepo.GetByIdWithLinesAsync(cmd.ShipmentId, ct);
            if (shipment is null)
                return ValidationResult<int>.NotFound($"Không tìm thấy lệnh xuất hàng #{cmd.ShipmentId}.");

            var line = shipment.AddLine(cmd.ProductCode, cmd.OrderedQty);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<int>.Ok(line.LineId);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
