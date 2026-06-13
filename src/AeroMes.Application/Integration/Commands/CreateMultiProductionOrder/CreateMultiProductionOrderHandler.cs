using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.CreateMultiProductionOrder;

public sealed class CreateMultiProductionOrderHandler(
    IMultiProductionOrderRepository repo,
    IProductRepository products,
    IValidator<CreateMultiProductionOrderCommand> validator,
    IUnitOfWork uow) : ICommandHandler<CreateMultiProductionOrderCommand, ValidationResult<MultiProductionOrderCreatedResult>>
{
    public async Task<ValidationResult<MultiProductionOrderCreatedResult>> HandleAsync(
        CreateMultiProductionOrderCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<MultiProductionOrderCreatedResult>.Invalid(vr.ToErrorDictionary());

        foreach (var line in cmd.Lines)
        {
            var product = await products.GetByCodeAsync(line.ProductCode.ToUpperInvariant(), ct);
            if (product is null)
                return ValidationResult<MultiProductionOrderCreatedResult>.NotFound(
                    $"Sản phẩm '{line.ProductCode}' không tồn tại.");
        }

        var count = await repo.CountAsync(ct);
        var year = DateTime.UtcNow.Year;
        var orderNumber = $"MPO-{year}-{count + 1:D4}";

        var mpo = MultiProductionOrder.Create(
            orderNumber,
            cmd.OrderType,
            cmd.SourceReference,
            cmd.PlannedStart,
            cmd.PlannedEnd,
            cmd.Priority,
            cmd.ProductionUnit,
            cmd.Notes,
            cmd.CreatedBy);

        foreach (var lineCmd in cmd.Lines)
            mpo.AddLine(lineCmd.ProductCode, lineCmd.PlannedQty, lineCmd.UoMCode, lineCmd.BomVersion);

        await repo.AddAsync(mpo, ct);
        await uow.SaveChangesAsync(ct);

        return ValidationResult<MultiProductionOrderCreatedResult>.Ok(
            new MultiProductionOrderCreatedResult(mpo.MPOId, mpo.OrderNumber, mpo.Lines.Count));
    }
}
