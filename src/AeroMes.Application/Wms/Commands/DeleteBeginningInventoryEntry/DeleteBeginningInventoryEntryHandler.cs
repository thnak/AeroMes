using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteBeginningInventoryEntry;

public class DeleteBeginningInventoryEntryHandler(
    IBeginningInventoryEntryRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<DeleteBeginningInventoryEntryCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteBeginningInventoryEntryCommand cmd, CancellationToken ct)
    {
        var entry = await repo.GetByIdAsync(cmd.EntryId, ct);
        if (entry is null)
            return ValidationResult<Unit>.NotFound($"Bản ghi tồn đầu kỳ '{cmd.EntryId}' không tồn tại.");

        entry.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);

        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
