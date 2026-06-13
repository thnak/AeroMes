using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.AddAvlItem;

public class AddAvlItemHandler(
    ISupplierRepository repo,
    IUnitOfWork uow,
    IValidator<AddAvlItemCommand> validator) : ICommandHandler<AddAvlItemCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddAvlItemCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var supplier = await repo.GetByIdWithAvlAsync(cmd.SupplierCode, ct)
                ?? throw new EntityNotFoundException(nameof(cmd.SupplierCode), cmd.SupplierCode);
            var item = supplier.AddAvlItem(
                cmd.ProductCode, cmd.Status,
                cmd.UnitPrice, cmd.CurrencyCode, cmd.LeadTimeDays,
                cmd.MinOrderQty, cmd.AqlLevel, cmd.IsPreferred,
                cmd.ApprovedFrom, cmd.ApprovedTo, cmd.Notes);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(item.AvlItemId);
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
