using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProductSpecification;

public class UpdateProductSpecificationHandler(
    IProductRepository repo,
    ISystemOptionsRepository optionsRepo,
    IUnitOfWork uow) : ICommandHandler<UpdateProductSpecificationCommand>
{
    public async Task HandleAsync(UpdateProductSpecificationCommand cmd, CancellationToken ct)
    {
        await optionsRepo.EnsureModeAsync(MaterialManagementModes.SpecificationCode, ct);

        var product = await repo.GetByCodeWithSpecificationsAsync(cmd.ProductCode, ct)
            ?? throw new EntityNotFoundException("Product", cmd.ProductCode);

        product.UpdateSpecification(cmd.SpecificationId, cmd.Description, cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
