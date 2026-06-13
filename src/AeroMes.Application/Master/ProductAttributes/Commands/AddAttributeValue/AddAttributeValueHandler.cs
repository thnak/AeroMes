using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.AddAttributeValue;

public class AddAttributeValueHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow,
    IValidator<AddAttributeValueCommand> validator) : ICommandHandler<AddAttributeValueCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddAttributeValueCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdWithValuesAsync(cmd.AttributeId, ct)
                ?? throw new EntityNotFoundException("ProductAttribute", cmd.AttributeId);

            var value = entity.AddValue(cmd.Value, cmd.GroupName, cmd.SortOrder, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(value.ValueId);
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
