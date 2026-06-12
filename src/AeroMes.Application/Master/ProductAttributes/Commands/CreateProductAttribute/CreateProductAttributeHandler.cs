using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.CreateProductAttribute;

public class CreateProductAttributeHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateProductAttributeCommand, int>
{
    public async Task<int> HandleAsync(CreateProductAttributeCommand cmd, CancellationToken ct)
    {
        var entity = ProductAttribute.Create(cmd.Code, cmd.Name, cmd.CreatedBy);
        foreach (var value in cmd.Values)
            entity.AddValue(value.Value, value.GroupName, value.SortOrder);

        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.AttributeId;
    }
}
