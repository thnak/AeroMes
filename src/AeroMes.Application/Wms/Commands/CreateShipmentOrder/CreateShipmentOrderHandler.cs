using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateShipmentOrder;

public class CreateShipmentOrderHandler(
    IShipmentOrderRepository shipmentRepo,
    ISalesOrderRepository soRepo,
    IUnitOfWork uow,
    IValidator<CreateShipmentOrderCommand> validator)
    : ICommandHandler<CreateShipmentOrderCommand, ValidationResult<ShipmentCreatedResult>>
{
    public async Task<ValidationResult<ShipmentCreatedResult>> HandleAsync(
        CreateShipmentOrderCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<ShipmentCreatedResult>.Invalid(validation.ToErrorDictionary());

        if (cmd.SoId.HasValue)
        {
            var so = await soRepo.GetByIdAsync(cmd.SoId.Value, ct);
            if (so is null)
                return ValidationResult<ShipmentCreatedResult>.NotFound(
                    $"Không tìm thấy đơn hàng #{cmd.SoId}.");
        }

        string code;
        int attempt = 0;
        do
        {
            var suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
            code = $"SHIP-{DateTime.UtcNow:yyyyMMdd}-{suffix}";
            attempt++;
        } while (await shipmentRepo.CodeExistsAsync(code, ct) && attempt < 5);

        var shipment = ShipmentOrder.Create(code, cmd.SoId, cmd.CustomerName, cmd.RequestedShipDate, cmd.CreatedBy);
        await shipmentRepo.AddAsync(shipment, ct);
        await uow.SaveChangesAsync(ct);

        return ValidationResult<ShipmentCreatedResult>.Ok(new ShipmentCreatedResult(shipment.ShipmentId, shipment.ShipmentCode));
    }
}
