using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Wms.Commands.ConfirmGrn;

public class ConfirmGrnHandler(
    IGoodsReceiptNoteRepository grnRepo,
    IPurchaseOrderRepository poRepo,
    IStockMovementRepository movementRepo,
    IInventoryStockRepository stockRepo,
    IUnitOfWork uow)
    : ICommandHandler<ConfirmGrnCommand, ValidationResult<Unit>>
{
    private const decimal OverDeliveryTolerancePct = 0.05m; // 5%

    public async Task<ValidationResult<Unit>> HandleAsync(ConfirmGrnCommand cmd, CancellationToken ct)
    {
        try
        {
            var grn = await grnRepo.GetByIdWithLinesAsync(cmd.GrnId, ct);
            if (grn is null) return ValidationResult<Unit>.NotFound($"GoodsReceiptNote '{cmd.GrnId}' was not found.");

            PurchaseOrder? po = null;
            if (grn.PoId.HasValue)
            {
                po = await poRepo.GetByIdWithLinesAsync(grn.PoId.Value, ct);
                if (po is null) return ValidationResult<Unit>.NotFound($"PurchaseOrder '{grn.PoId.Value}' was not found.");

                var errors = ValidateQtyAgainstPo(grn, po);
                if (errors.Count > 0)
                    return ValidationResult<Unit>.Invalid(errors);
            }

            foreach (var line in grn.Lines)
            {
                var stock = await stockRepo.FindByKeyAsync(grn.StorageLocationId, line.ProductCode, line.LotNumber, ct);
                if (stock is null)
                {
                    stock = InventoryStock.Create(grn.StorageLocationId, line.ProductCode, line.LotNumber);
                    await stockRepo.AddAsync(stock, ct);
                }

                stock.SetBin(line.DestinationBinId);
                stock.Adjust(line.ReceivedQty);

                var movement = StockMovement.CreateReceive(
                    line.ProductCode, line.LotNumber, line.ReceivedQty,
                    grn.StorageLocationId, line.DestinationBinId,
                    grn.GrnCode, $"GRN line {line.GrnLineId}", cmd.ConfirmedBy);
                await movementRepo.AddAsync(movement, ct);
            }

            if (po is not null)
            {
                foreach (var line in grn.Lines.Where(l => l.PoLineId.HasValue))
                    po.RecordLineReceived(line.PoLineId!.Value, line.ReceivedQty, cmd.ConfirmedBy);
            }

            grn.Confirm(cmd.ConfirmedBy);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }

    private static Dictionary<string, string[]> ValidateQtyAgainstPo(GoodsReceiptNote grn, PurchaseOrder po)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var line in grn.Lines.Where(l => l.PoLineId.HasValue))
        {
            var poLine = po.Lines.FirstOrDefault(pl => pl.PoLineId == line.PoLineId);
            if (poLine is null)
                continue;

            var allowedMax = poLine.OrderedQty * (1 + OverDeliveryTolerancePct);
            if (poLine.ReceivedQty + line.ReceivedQty > allowedMax)
                errors[$"line_{line.GrnLineId}"] =
                [
                    $"Received qty {poLine.ReceivedQty + line.ReceivedQty} exceeds ordered qty {poLine.OrderedQty} + tolerance for product '{line.ProductCode}'."
                ];
        }

        return errors;
    }
}
