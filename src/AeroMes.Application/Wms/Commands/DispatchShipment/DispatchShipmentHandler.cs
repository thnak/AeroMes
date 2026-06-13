using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DispatchShipment;

public class DispatchShipmentHandler(
    IShipmentOrderRepository shipmentRepo,
    IPickListRepository pickListRepo,
    IInventoryStockRepository stockRepo,
    IStockMovementRepository movementRepo,
    ISalesOrderRepository soRepo,
    IUnitOfWork uow,
    IValidator<DispatchShipmentCommand> validator)
    : ICommandHandler<DispatchShipmentCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DispatchShipmentCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var shipment = await shipmentRepo.GetByIdWithLinesAsync(cmd.ShipmentId, ct);
            if (shipment is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy lệnh xuất hàng #{cmd.ShipmentId}.");

            var pickList = await pickListRepo.GetByShipmentIdAsync(cmd.ShipmentId, ct);
            if (pickList is null || pickList.Status != PickListStatus.Completed)
                return ValidationResult<Unit>.Failure(
                    "Phiếu lấy hàng chưa hoàn thành. Vui lòng hoàn thành phiếu lấy hàng trước khi xuất hàng.");

            // Create ISSUE stock movements for each confirmed pick line
            foreach (var pickLine in pickList.Lines.Where(l => l.IsConfirmed && l.PickedQty > 0))
            {
                var stock = await stockRepo.FindByKeyAsync(
                    pickLine.LocationId, pickLine.ProductCode, pickLine.LotNumber, ct);

                if (stock is not null)
                    stock.Adjust(-pickLine.PickedQty);

                var movement = StockMovement.CreateIssue(
                    pickLine.ProductCode, pickLine.LotNumber, pickLine.PickedQty,
                    pickLine.LocationId, pickLine.BinId,
                    shipment.ShipmentCode,
                    $"Dispatch shipment {shipment.ShipmentCode}",
                    cmd.DispatchedBy);
                await movementRepo.AddAsync(movement, ct);
            }

            shipment.Dispatch(cmd.CarrierName, cmd.TrackingNumber, cmd.DispatchedBy);

            // Close sales order if fully fulfilled and all lines shipped
            if (shipment.SoId.HasValue)
            {
                var so = await soRepo.GetByIdAsync(shipment.SoId.Value, ct);
                if (so is not null && shipment.Lines.All(l => l.PickedQty >= l.OrderedQty))
                    so.Close();
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
