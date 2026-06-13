using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateBeginningInventoryEntry;

public class UpdateBeginningInventoryEntryHandler(
    IBeginningInventoryEntryRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateBeginningInventoryEntryCommand> validator)
    : ICommandHandler<UpdateBeginningInventoryEntryCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateBeginningInventoryEntryCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var entry = await repo.GetByIdAsync(cmd.EntryId, ct);
        if (entry is null)
            return ValidationResult<Unit>.NotFound(
                $"Bản ghi tồn đầu kỳ '{cmd.EntryId}' không tồn tại.");

        try
        {
            entry.Update(cmd.BeginningQuantity, cmd.LotNumber, cmd.ExpirationDate, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
