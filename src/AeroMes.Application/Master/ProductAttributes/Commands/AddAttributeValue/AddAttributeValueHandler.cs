using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.AddAttributeValue;

public class AddAttributeValueHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow) : ICommandHandler<AddAttributeValueCommand, int>
{
    public async Task<int> HandleAsync(AddAttributeValueCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdWithValuesAsync(cmd.AttributeId, ct)
            ?? throw new EntityNotFoundException("ProductAttribute", cmd.AttributeId);

        var value = entity.AddValue(cmd.Value, cmd.GroupName, cmd.SortOrder, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return value.ValueId;
    }
}
