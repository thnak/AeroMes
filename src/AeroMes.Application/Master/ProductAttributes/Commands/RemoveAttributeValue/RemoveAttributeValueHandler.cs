using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.RemoveAttributeValue;

public class RemoveAttributeValueHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveAttributeValueCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemoveAttributeValueCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdWithValuesAsync(cmd.AttributeId, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"ProductAttribute '{cmd.AttributeId}' was not found.");

        if (await repo.IsValueInUseAsync(cmd.ValueId, ct))
            throw new DomainException(
                $"Giá trị #{cmd.ValueId} đang được sản phẩm sử dụng — không thể xóa.");

        entity.RemoveValue(cmd.ValueId, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
