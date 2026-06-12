using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.RemoveAttributeValue;

public class RemoveAttributeValueHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveAttributeValueCommand>
{
    public async Task HandleAsync(RemoveAttributeValueCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdWithValuesAsync(cmd.AttributeId, ct)
            ?? throw new EntityNotFoundException("ProductAttribute", cmd.AttributeId);

        if (await repo.IsValueInUseAsync(cmd.ValueId, ct))
            throw new DomainException(
                $"Giá trị #{cmd.ValueId} đang được sản phẩm sử dụng — không thể xóa.");

        entity.RemoveValue(cmd.ValueId, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
