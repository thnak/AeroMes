using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.AddProductSpecification;

public class AddProductSpecificationHandler(
    IProductRepository repo,
    ISystemOptionsRepository optionsRepo,
    IUnitOfWork uow) : ICommandHandler<AddProductSpecificationCommand, int>
{
    public async Task<int> HandleAsync(AddProductSpecificationCommand cmd, CancellationToken ct)
    {
        await optionsRepo.EnsureModeAsync(MaterialManagementModes.SpecificationCode, ct);

        var product = await repo.GetByCodeWithSpecificationsAsync(cmd.ProductCode, ct)
            ?? throw new EntityNotFoundException("Product", cmd.ProductCode);

        var spec = product.AddSpecification(cmd.SpecCode, cmd.Description, cmd.CreatedBy);
        await uow.SaveChangesAsync(ct);
        return spec.SpecificationId;
    }
}
