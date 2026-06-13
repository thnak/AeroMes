using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Signals.Commands.DeleteSignalTag;

public class DeleteSignalTagHandler(
    ISignalTagRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteSignalTagCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteSignalTagCommand cmd, CancellationToken ct)
    {
        var tag = await repo.GetByKeyAsync(cmd.Key, ct);
        if (tag is null)
            return ValidationResult<Unit>.NotFound($"Signal tag '{cmd.Key}' not found.");

        if (await repo.IsInUseAsync(cmd.Key, ct))
            return ValidationResult<Unit>.Failure($"Signal tag '{cmd.Key}' is referenced by one or more signal mappings and cannot be deleted.");

        try
        {
            tag.GuardDelete();
            repo.Remove(tag);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
