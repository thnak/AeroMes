using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Master;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.BomItems.Commands.CreateBomItem;
public class CreateBomItemHandler(
    IBomItemRepository repo,
    IUnitOfWork uow,
    IValidator<CreateBomItemCommand> validator) : ICommandHandler<CreateBomItemCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateBomItemCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());
        try
        {
            var entity = BomItem.Create(
                cmd.ParentProductCode,
                cmd.ChildProductCode,
                cmd.RequiredQty,
                cmd.ScrapFactor,
                cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.BomID);
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
