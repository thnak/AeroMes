using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.DeleteProductAttribute;

public class DeleteProductAttributeHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteProductAttributeCommand>
{
    public async Task HandleAsync(DeleteProductAttributeCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.AttributeId, ct)
            ?? throw new EntityNotFoundException("ProductAttribute", cmd.AttributeId);

        if (await repo.HasAssignmentsAsync(cmd.AttributeId, ct))
            throw new DomainException(
                $"Thuộc tính '{entity.AttributeCode}' đang được gán cho sản phẩm — chỉ có thể vô hiệu hóa, không thể xóa.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
