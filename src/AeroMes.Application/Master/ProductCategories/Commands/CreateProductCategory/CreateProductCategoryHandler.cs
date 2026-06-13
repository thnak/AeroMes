using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.ProductCategories.Commands.CreateProductCategory;

public class CreateProductCategoryHandler(
    IProductCategoryRepository repo,
    IUnitOfWork uow,
    IValidator<CreateProductCategoryCommand> validator) : ICommandHandler<CreateProductCategoryCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateProductCategoryCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = ProductCategory.Create(
                cmd.ParentId, cmd.Code, cmd.Name,
                cmd.Description, cmd.StandardProductionTime, cmd.Color,
                cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.CategoryId);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
