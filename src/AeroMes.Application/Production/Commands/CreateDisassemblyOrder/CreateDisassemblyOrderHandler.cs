using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreateDisassemblyOrder;

public class CreateDisassemblyOrderHandler(
    IDisassemblyOrderRepository repo,
    IDisassemblyBomRepository bomRepo,
    IValidator<CreateDisassemblyOrderCommand> validator)
    : ICommandHandler<CreateDisassemblyOrderCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateDisassemblyOrderCommand cmd, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(cmd, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        var bom = await bomRepo.GetDefaultBySourceProductAsync(cmd.SourceProductCode, ct);
        if (bom is not null)
            bom = await bomRepo.GetByIdWithLinesAsync(bom.DisassemblyBomId, ct);
        if (bom is null)
            return ValidationResult<int>.Failure($"No active DisassemblyBom found for product '{cmd.SourceProductCode}'.");

        var orderCode = $"DASM-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var recoveredLines = bom.Lines.Select(l =>
            (l.ComponentCode, l.FixedQuantity ?? l.RecoveryRate ?? 1m));

        try
        {
            var order = DisassemblyOrder.Create(
                orderCode, cmd.OrderType, cmd.SourceProductCode, bom.DisassemblyBomId,
                cmd.SourceQty, recoveredLines, cmd.PurchaseOrderID, cmd.Deadline, cmd.Notes);
            await repo.AddAsync(order, ct);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(order.DisassemblyOrderID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
