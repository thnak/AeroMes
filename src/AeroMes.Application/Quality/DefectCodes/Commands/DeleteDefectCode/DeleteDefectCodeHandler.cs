using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Quality.DefectCodes.Commands.DeleteDefectCode;

public class DeleteDefectCodeHandler(
    IDefectCodeRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteDefectCodeCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteDefectCodeCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"DefectCode '{cmd.Id}' was not found.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
