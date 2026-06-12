using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.DefectCodes.Commands.UpdateDefectCode;

public class UpdateDefectCodeHandler(
    IDefectCodeRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateDefectCodeCommand>
{
    public async Task HandleAsync(UpdateDefectCodeCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new EntityNotFoundException("DefectCode", cmd.Id);

        entity.UpdateDetails(cmd.DefectName, cmd.DefectCategory, cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
