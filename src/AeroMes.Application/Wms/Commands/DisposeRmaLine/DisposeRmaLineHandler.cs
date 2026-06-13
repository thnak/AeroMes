using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DisposeRmaLine;

public class DisposeRmaLineHandler(
    IRmaRepository rmaRepo,
    IInventoryStockRepository stockRepo,
    IStockMovementRepository movementRepo,
    IUnitOfWork uow,
    IValidator<DisposeRmaLineCommand> validator)
    : ICommandHandler<DisposeRmaLineCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DisposeRmaLineCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var rma = await rmaRepo.GetByIdWithLinesAsync(cmd.RmaId, ct);
            if (rma is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy RMA #{cmd.RmaId}.");

            var line = rma.Lines.FirstOrDefault(l => l.RmaLineId == cmd.LineId);
            if (line is null)
                return ValidationResult<Unit>.NotFound(
                    $"Không tìm thấy dòng #{cmd.LineId} trong RMA #{cmd.RmaId}.");

            var qty = line.ReceivedQty > 0 ? line.ReceivedQty : line.ReturnQty;

            long? movementId = null;

            switch (cmd.Disposition)
            {
                case RmaDisposition.Scrap:
                    // Write off stock
                    movementId = await AdjustStockAsync(
                        line.ProductCode, line.LotNumber, -qty,
                        cmd.DispositionLocationId, cmd.DispositionBinId,
                        rma.RmaCode, cmd.Notes ?? $"RMA scrap: {rma.RmaCode}", cmd.DisposedBy, ct);
                    break;

                case RmaDisposition.ReturnToSupplier:
                    // Remove from inventory (supplier return direction only)
                    movementId = await AdjustStockAsync(
                        line.ProductCode, line.LotNumber, -qty,
                        cmd.DispositionLocationId, cmd.DispositionBinId,
                        rma.RmaCode, cmd.Notes ?? $"Return to supplier: {rma.RmaCode}", cmd.DisposedBy, ct);
                    break;

                case RmaDisposition.ReturnToStock:
                    // Re-integrate into usable stock (customer return — already in quarantine)
                    movementId = await AdjustStockAsync(
                        line.ProductCode, line.LotNumber, qty,
                        cmd.DispositionLocationId, cmd.DispositionBinId,
                        rma.RmaCode, cmd.Notes ?? $"RMA return to stock: {rma.RmaCode}", cmd.DisposedBy, ct);
                    break;

                case RmaDisposition.Rework:
                    // Stock stays in quarantine/rework area; movement records the placement
                    movementId = await AdjustStockAsync(
                        line.ProductCode, line.LotNumber, qty,
                        cmd.DispositionLocationId, cmd.DispositionBinId,
                        rma.RmaCode, cmd.Notes ?? $"RMA rework: {rma.RmaCode}", cmd.DisposedBy, ct);
                    break;

                case RmaDisposition.Quarantine:
                    // No stock movement — goods stay put
                    break;
            }

            rma.DisposeLine(cmd.LineId, cmd.Disposition, movementId);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }

    private async Task<long?> AdjustStockAsync(
        string productCode, string lotNumber, decimal delta,
        int locationId, int? binId,
        string reference, string? notes, string? by, CancellationToken ct)
    {
        var stock = await stockRepo.FindByKeyAsync(locationId, productCode, lotNumber, ct);
        if (stock is null)
        {
            if (delta > 0)
            {
                stock = InventoryStock.Create(locationId, productCode, lotNumber, delta);
                await stockRepo.AddAsync(stock, ct);
            }
        }
        else
        {
            stock.Adjust(delta);
        }

        var movement = StockMovement.CreateAdjust(
            productCode, lotNumber, delta,
            locationId, binId, reference, notes, by);

        await movementRepo.AddAsync(movement, ct);
        return movement.MovementId;
    }
}
