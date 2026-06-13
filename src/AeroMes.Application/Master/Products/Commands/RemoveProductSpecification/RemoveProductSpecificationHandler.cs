using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Products.Commands.RemoveProductSpecification;

public class RemoveProductSpecificationHandler(
    IProductRepository repo,
    ISystemOptionsRepository optionsRepo,
    IUnitOfWork uow) : ICommandHandler<RemoveProductSpecificationCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemoveProductSpecificationCommand cmd, CancellationToken ct)
    {
        await optionsRepo.EnsureModeAsync(MaterialManagementModes.SpecificationCode, ct);

        var product = await repo.GetByCodeWithSpecificationsAsync(cmd.ProductCode, ct);
        if (product is null) return ValidationResult<Unit>.NotFound($"Product '{cmd.ProductCode}' was not found.");

        product.RemoveSpecification(cmd.SpecificationId, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
