using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateCarton;

public class CreateCartonHandler(
    IShipmentOrderRepository shipmentRepo,
    ICartonRepository cartonRepo,
    IUnitOfWork uow,
    IValidator<CreateCartonCommand> validator)
    : ICommandHandler<CreateCartonCommand, ValidationResult<CartonCreatedResult>>
{
    public async Task<ValidationResult<CartonCreatedResult>> HandleAsync(
        CreateCartonCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<CartonCreatedResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var shipment = await shipmentRepo.GetByIdAsync(cmd.ShipmentId, ct);
            if (shipment is null)
                return ValidationResult<CartonCreatedResult>.NotFound(
                    $"Không tìm thấy lệnh xuất hàng #{cmd.ShipmentId}.");

            if (shipment.Status == ShipmentStatus.Dispatched || shipment.Status == ShipmentStatus.Cancelled)
                return ValidationResult<CartonCreatedResult>.Failure(
                    $"Lệnh xuất hàng '{shipment.ShipmentCode}' không thể tạo thùng ở trạng thái hiện tại.");

            var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            var cartonCode = $"CTN-{shipment.ShipmentCode}-{suffix}";

            var carton = Carton.Create(cmd.ShipmentId, cartonCode, cmd.CreatedBy);
            await cartonRepo.AddAsync(carton, ct);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<CartonCreatedResult>.Ok(new CartonCreatedResult(carton.CartonId, carton.CartonCode));
        }
        catch (DomainException ex)
        {
            return ValidationResult<CartonCreatedResult>.Failure(ex.Message);
        }
    }
}
