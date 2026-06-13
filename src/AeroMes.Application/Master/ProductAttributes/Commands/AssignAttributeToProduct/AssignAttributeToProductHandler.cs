using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.AssignAttributeToProduct;

public class AssignAttributeToProductHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow,
    IValidator<AssignAttributeToProductCommand> validator) : ICommandHandler<AssignAttributeToProductCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AssignAttributeToProductCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var attribute = await repo.GetByIdWithValuesAsync(cmd.AttributeId, ct);
            if (attribute is null)
                return ValidationResult<int>.NotFound($"ProductAttribute {cmd.AttributeId} not found.");

            if (!attribute.IsActive)
                return ValidationResult<int>.Failure($"Thuộc tính '{attribute.AttributeCode}' đã bị vô hiệu hóa — không thể gán cho sản phẩm.");

            if (cmd.SelectedValueId is int valueId && attribute.Values.All(v => v.ValueId != valueId))
                return ValidationResult<int>.Failure($"Giá trị #{valueId} không thuộc thuộc tính '{attribute.AttributeCode}'.");

            var existing = await repo.GetAssignmentAsync(cmd.ProductCode, cmd.AttributeId, ct);
            if (existing is not null)
            {
                existing.SelectValue(cmd.SelectedValueId, cmd.CreatedBy);
                await uow.SaveChangesAsync(ct);
                return ValidationResult<int>.Ok(existing.AssignmentId);
            }

            var assignment = ProductAttributeAssignment.Create(cmd.ProductCode, cmd.AttributeId, cmd.SelectedValueId, cmd.CreatedBy);
            await repo.AddAssignmentAsync(assignment, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(assignment.AssignmentId);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<int>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
