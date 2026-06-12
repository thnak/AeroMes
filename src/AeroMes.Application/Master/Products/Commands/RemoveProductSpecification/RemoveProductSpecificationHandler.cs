using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.RemoveProductSpecification;

public class RemoveProductSpecificationHandler(
    IProductRepository repo,
    ISystemOptionsRepository optionsRepo,
    IUnitOfWork uow) : ICommandHandler<RemoveProductSpecificationCommand>
{
    public async Task HandleAsync(RemoveProductSpecificationCommand cmd, CancellationToken ct)
    {
        await optionsRepo.EnsureModeAsync(MaterialManagementModes.SpecificationCode, ct);

        var product = await repo.GetByCodeWithSpecificationsAsync(cmd.ProductCode, ct)
            ?? throw new EntityNotFoundException("Product", cmd.ProductCode);

        product.RemoveSpecification(cmd.SpecificationId, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
