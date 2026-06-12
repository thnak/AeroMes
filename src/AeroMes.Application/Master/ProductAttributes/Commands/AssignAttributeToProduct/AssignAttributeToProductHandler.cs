using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.AssignAttributeToProduct;

public class AssignAttributeToProductHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow) : ICommandHandler<AssignAttributeToProductCommand, int>
{
    public async Task<int> HandleAsync(AssignAttributeToProductCommand cmd, CancellationToken ct)
    {
        var attribute = await repo.GetByIdWithValuesAsync(cmd.AttributeId, ct)
            ?? throw new EntityNotFoundException("ProductAttribute", cmd.AttributeId);

        if (!attribute.IsActive)
            throw new DomainException($"Thuộc tính '{attribute.AttributeCode}' đã bị vô hiệu hóa — không thể gán cho sản phẩm.");

        if (cmd.SelectedValueId is int valueId && attribute.Values.All(v => v.ValueId != valueId))
            throw new DomainException($"Giá trị #{valueId} không thuộc thuộc tính '{attribute.AttributeCode}'.");

        var existing = await repo.GetAssignmentAsync(cmd.ProductCode, cmd.AttributeId, ct);
        if (existing is not null)
        {
            existing.SelectValue(cmd.SelectedValueId, cmd.CreatedBy);
            await uow.SaveChangesAsync(ct);
            return existing.AssignmentId;
        }

        var assignment = ProductAttributeAssignment.Create(cmd.ProductCode, cmd.AttributeId, cmd.SelectedValueId, cmd.CreatedBy);
        await repo.AddAssignmentAsync(assignment, ct);
        await uow.SaveChangesAsync(ct);
        return assignment.AssignmentId;
    }
}
