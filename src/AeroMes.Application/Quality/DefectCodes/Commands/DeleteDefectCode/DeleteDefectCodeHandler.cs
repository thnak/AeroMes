using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.DefectCodes.Commands.DeleteDefectCode;

public class DeleteDefectCodeHandler(
    IDefectCodeRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteDefectCodeCommand>
{
    public async Task HandleAsync(DeleteDefectCodeCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new EntityNotFoundException("DefectCode", cmd.Id);

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
