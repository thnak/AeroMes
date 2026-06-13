using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.DeleteProductAttribute;

public class DeleteProductAttributeHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteProductAttributeCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteProductAttributeCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.AttributeId, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"ProductAttribute '{cmd.AttributeId}' was not found.");

        if (await repo.HasAssignmentsAsync(cmd.AttributeId, ct))
            throw new DomainException(
                $"Thuộc tính '{entity.AttributeCode}' đang được gán cho sản phẩm — chỉ có thể vô hiệu hóa, không thể xóa.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
