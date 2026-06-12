using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.UpdateAttributeValue;

public class UpdateAttributeValueHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateAttributeValueCommand>
{
    public async Task HandleAsync(UpdateAttributeValueCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdWithValuesAsync(cmd.AttributeId, ct)
            ?? throw new EntityNotFoundException("ProductAttribute", cmd.AttributeId);

        entity.UpdateValue(cmd.ValueId, cmd.Value, cmd.GroupName, cmd.SortOrder, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
