using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.CreateProductAttribute;

public class CreateProductAttributeHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow,
    IValidator<CreateProductAttributeCommand> validator) : ICommandHandler<CreateProductAttributeCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateProductAttributeCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = ProductAttribute.Create(cmd.Code, cmd.Name, cmd.CreatedBy);
            foreach (var value in cmd.Values)
                entity.AddValue(value.Value, value.GroupName, value.SortOrder);

            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.AttributeId);
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
