using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Application.Wms.Services;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreatePickList;

public class CreatePickListHandler(
    IShipmentOrderRepository shipmentRepo,
    IPickListRepository pickListRepo,
    ILotAllocationService allocationService,
    IUnitOfWork uow,
    IValidator<CreatePickListCommand> validator)
    : ICommandHandler<CreatePickListCommand, ValidationResult<PickListCreatedResult>>
{
    public async Task<ValidationResult<PickListCreatedResult>> HandleAsync(
        CreatePickListCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<PickListCreatedResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var shipment = await shipmentRepo.GetByIdWithLinesAsync(cmd.ShipmentId, ct);
            if (shipment is null)
                return ValidationResult<PickListCreatedResult>.NotFound(
                    $"Không tìm thấy lệnh xuất hàng #{cmd.ShipmentId}.");

            if (shipment.Status == ShipmentStatus.Dispatched || shipment.Status == ShipmentStatus.Cancelled)
                return ValidationResult<PickListCreatedResult>.Failure(
                    $"Lệnh xuất hàng '{shipment.ShipmentCode}' không thể tạo phiếu lấy hàng ở trạng thái hiện tại.");

            if (shipment.Lines.Count == 0)
                return ValidationResult<PickListCreatedResult>.Failure(
                    $"Lệnh xuất hàng '{shipment.ShipmentCode}' chưa có dòng nào.");

            var pickList = PickList.Create(cmd.ShipmentId, cmd.AssignedTo, cmd.CreatedBy);
            await pickListRepo.AddAsync(pickList, ct);
            await uow.SaveChangesAsync(ct);

            int sequence = 1;
            bool allFulfilled = true;

            foreach (var line in shipment.Lines)
            {
                var allocation = await allocationService.AllocateAsync(
                    line.ProductCode, line.OrderedQty, cmd.LocationId, null, ct);

                if (!allocation.IsFulfilled)
                    allFulfilled = false;

                foreach (var lot in allocation.Allocations)
                {
                    pickList.AddLine(
                        line.LineId,
                        line.ProductCode,
                        lot.LotNumber,
                        lot.LocationId,
                        lot.BinId,
                        lot.AllocatedQty,
                        sequence++);
                }
            }

            if (shipment.Status == ShipmentStatus.Draft)
                shipment.StartPicking(cmd.CreatedBy);

            await uow.SaveChangesAsync(ct);

            return ValidationResult<PickListCreatedResult>.Ok(
                new PickListCreatedResult(pickList.PickListId, pickList.Lines.Count, allFulfilled));
        }
        catch (DomainException ex)
        {
            return ValidationResult<PickListCreatedResult>.Failure(ex.Message);
        }
    }
}
