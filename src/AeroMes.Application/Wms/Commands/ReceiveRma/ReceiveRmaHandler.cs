using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ReceiveRma;

public class ReceiveRmaHandler(
    IRmaRepository rmaRepo,
    IInventoryStockRepository stockRepo,
    IStockMovementRepository movementRepo,
    IUnitOfWork uow,
    IValidator<ReceiveRmaCommand> validator)
    : ICommandHandler<ReceiveRmaCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        ReceiveRmaCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var rma = await rmaRepo.GetByIdWithLinesAsync(cmd.RmaId, ct);
            if (rma is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy RMA #{cmd.RmaId}.");

            var tuples = cmd.LineReceipts
                .Select(r => (r.LineId, r.ReceivedQty))
                .ToList();

            rma.MarkReceived(cmd.ReceivedBy ?? "system", tuples);

            // For CUSTOMER_RETURN: received goods enter quarantine stock
            if (rma.ReturnDirection == ReturnDirection.CustomerReturn)
            {
                foreach (var receipt in cmd.LineReceipts.Where(r => r.ReceivedQty > 0))
                {
                    var line = rma.Lines.First(l => l.RmaLineId == receipt.LineId);

                    var stock = await stockRepo.FindByKeyAsync(
                        cmd.QuarantineLocationId, line.ProductCode, line.LotNumber, ct);

                    if (stock is null)
                    {
                        stock = InventoryStock.Create(
                            cmd.QuarantineLocationId, line.ProductCode, line.LotNumber,
                            receipt.ReceivedQty);
                        await stockRepo.AddAsync(stock, ct);
                    }
                    else
                    {
                        stock.Adjust(receipt.ReceivedQty);
                    }

                    var movement = StockMovement.CreateReceive(
                        line.ProductCode, line.LotNumber, receipt.ReceivedQty,
                        cmd.QuarantineLocationId, cmd.QuarantineBinId,
                        rma.RmaCode, $"Customer return receipt", cmd.ReceivedBy);

                    await movementRepo.AddAsync(movement, ct);
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
