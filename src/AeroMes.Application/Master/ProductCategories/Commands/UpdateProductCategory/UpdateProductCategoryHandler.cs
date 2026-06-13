using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.ProductCategories.Commands.UpdateProductCategory;

public class UpdateProductCategoryHandler(
    IProductCategoryRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateProductCategoryCommand> validator) : ICommandHandler<UpdateProductCategoryCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateProductCategoryCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdAsync(cmd.Id, ct);
            if (entity is null) return ValidationResult<Unit>.NotFound($"ProductCategory '{cmd.Id}' was not found.");

            if (cmd.ParentId is int parentId)
                await EnsureNoCycleAsync(cmd.Id, parentId, ct);

            entity.UpdateDetails(
                cmd.ParentId, cmd.Name,
                cmd.Description, cmd.StandardProductionTime, cmd.Color,
                cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }

    private async Task EnsureNoCycleAsync(int categoryId, int parentId, CancellationToken ct)
    {
        var current = (int?)parentId;
        while (current is int id)
        {
            if (id == categoryId)
                throw new DomainException("Nhóm cha không hợp lệ — tạo vòng lặp trong cây phân cấp.");
            var parent = await repo.GetByIdAsync(id, ct);
            if (parent is null) return;
            current = parent.ParentId;
        }
    }
}
