using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.UpdateProductAttribute;

public class UpdateProductAttributeHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateProductAttributeCommand>
{
    public async Task HandleAsync(UpdateProductAttributeCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.AttributeId, ct)
            ?? throw new EntityNotFoundException("ProductAttribute", cmd.AttributeId);
        entity.UpdateDetails(cmd.Name, cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
