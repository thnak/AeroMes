using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Commands.DeleteDowntimeReasonCode;

public class DeleteDowntimeReasonCodeHandler(
    IDowntimeReasonCodeRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteDowntimeReasonCodeCommand>
{
    public async Task HandleAsync(DeleteDowntimeReasonCodeCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("DowntimeReasonCode", cmd.Code);

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
