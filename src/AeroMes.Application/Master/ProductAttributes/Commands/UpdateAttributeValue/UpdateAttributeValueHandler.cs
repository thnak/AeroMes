using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.UpdateAttributeValue;

public class UpdateAttributeValueHandler(
    IProductAttributeRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateAttributeValueCommand> validator) : ICommandHandler<UpdateAttributeValueCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateAttributeValueCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdWithValuesAsync(cmd.AttributeId, ct);
            if (entity is null) return ValidationResult<Unit>.NotFound($"ProductAttribute '{cmd.AttributeId}' was not found.");

            entity.UpdateValue(cmd.ValueId, cmd.Value, cmd.GroupName, cmd.SortOrder, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
