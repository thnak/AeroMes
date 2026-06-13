using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreatePackagingOrder;

public class CreatePackagingOrderHandler(
    IPackagingRepository repo,
    IWorkOrderRepository woRepo,
    IValidator<CreatePackagingOrderCommand> validator)
    : ICommandHandler<CreatePackagingOrderCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreatePackagingOrderCommand cmd, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(cmd, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        var wo = await woRepo.GetByIdAsync(cmd.WOID, ct);
        if (wo is null) return ValidationResult<int>.NotFound($"WorkOrder '{cmd.WOID}' not found.");

        var bom = await repo.GetActiveBomForProductAsync(cmd.ProductCode, ct);
        if (bom is null)
            return ValidationResult<int>.Failure($"No active PackagingBom found for product '{cmd.ProductCode}'.");

        var idCode = $"PKG-{cmd.WOID}-{DateTime.UtcNow:yyyyMMddHHmmss}";

        try
        {
            var order = PackagingOrder.Create(cmd.WOID, bom.PackagingBomID, cmd.ProductCode, cmd.PlannedQty, idCode, cmd.Notes);
            await repo.AddOrderAsync(order, ct);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(order.PackagingOrderID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
