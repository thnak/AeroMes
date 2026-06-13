using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.UpdateAvlItem;

public class UpdateAvlItemHandler(
    ISupplierRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateAvlItemCommand> validator) : ICommandHandler<UpdateAvlItemCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateAvlItemCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var supplier = await repo.GetByIdWithAvlAsync(cmd.SupplierCode, ct)
                ?? throw new EntityNotFoundException(nameof(cmd.SupplierCode), cmd.SupplierCode);
            supplier.UpdateAvlItem(
                cmd.AvlItemId, cmd.Status,
                cmd.UnitPrice, cmd.CurrencyCode, cmd.LeadTimeDays,
                cmd.MinOrderQty, cmd.AqlLevel, cmd.IsPreferred,
                cmd.ApprovedFrom, cmd.ApprovedTo, cmd.Notes);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<Unit>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
