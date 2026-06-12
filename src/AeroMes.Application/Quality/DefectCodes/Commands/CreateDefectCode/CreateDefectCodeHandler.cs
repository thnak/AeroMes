using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.DefectCodes.Commands.CreateDefectCode;

public class CreateDefectCodeHandler(
    IDefectCodeRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateDefectCodeCommand, int>
{
    public async Task<int> HandleAsync(CreateDefectCodeCommand cmd, CancellationToken ct)
    {
        var entity = DefectCode.Create(cmd.Code, cmd.DefectName, cmd.DefectCategory, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.DefectCodeID;
    }
}
