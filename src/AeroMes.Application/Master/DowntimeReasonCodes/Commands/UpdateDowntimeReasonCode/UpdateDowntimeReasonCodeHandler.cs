using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Commands.UpdateDowntimeReasonCode;

public class UpdateDowntimeReasonCodeHandler(
    IDowntimeReasonCodeRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateDowntimeReasonCodeCommand>
{
    public async Task HandleAsync(UpdateDowntimeReasonCodeCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("DowntimeReasonCode", cmd.Code);

        entity.UpdateDetails(cmd.Name, cmd.Category, cmd.SlaMinutes, cmd.RequiresApproval, cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
