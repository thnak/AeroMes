using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.BatchCreateProductionOrders;

public sealed class BatchCreateProductionOrdersHandler(
    IProductionOrderRepository repo,
    IProductRepository products,
    IValidator<BatchCreateProductionOrdersCommand> validator,
    IUnitOfWork uow) : ICommandHandler<BatchCreateProductionOrdersCommand, ValidationResult<BatchCreateResult>>
{
    public async Task<ValidationResult<BatchCreateResult>> HandleAsync(
        BatchCreateProductionOrdersCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<BatchCreateResult>.Invalid(vr.ToErrorDictionary());

        // Verify all products exist
        foreach (var item in cmd.Items)
        {
            var product = await products.GetByCodeAsync(item.ProductCode.ToUpperInvariant(), ct);
            if (product is null)
                return ValidationResult<BatchCreateResult>.NotFound($"Product '{item.ProductCode}' not found.");
        }

        var baseCount = await repo.CountAsync(ct);
        var year = DateTime.UtcNow.Year;
        var created = new List<string>();

        foreach (var (item, index) in cmd.Items.Select((x, i) => (x, i)))
        {
            var poCode = $"PO-{year}-{baseCount + index + 1:D4}";
            var po = ProductionOrder.CreateInternal(
                poCode,
                item.ProductCode,
                item.TargetQuantity,
                item.PlannedStartDate,
                item.PlannedEndDate,
                item.ProductionDeadline,
                item.Priority,
                item.AssignedTo,
                cmd.CreatedBy);
            await repo.AddAsync(po, ct);
            created.Add(poCode);
        }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<BatchCreateResult>.Ok(new BatchCreateResult(created.Count, created));
    }
}
