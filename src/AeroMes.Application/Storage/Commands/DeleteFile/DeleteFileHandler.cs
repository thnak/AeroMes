using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Storage.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Storage.Commands.DeleteFile;

public sealed class DeleteFileHandler(
    IFileStorage fileStorage,
    IFileObjectRepository repo,
    IUnitOfWork uow
) : ICommandHandler<DeleteFileCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteFileCommand cmd, CancellationToken ct)
    {
        var file = await repo.GetByIdAsync(cmd.Id, ct);
        if (file is null) return ValidationResult<Unit>.NotFound("File not found.");

        file.SoftDelete();
        await fileStorage.DeleteAsync(file.StorageKey, ct);
        await uow.SaveChangesAsync(ct);

        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
