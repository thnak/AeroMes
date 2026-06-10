using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Commands.CreateDowntimeReasonCode;

public class CreateDowntimeReasonCodeHandler(
    IDowntimeReasonCodeRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateDowntimeReasonCodeCommand, string>
{
    public async Task<string> HandleAsync(CreateDowntimeReasonCodeCommand cmd, CancellationToken ct)
    {
        var entity = DowntimeReasonCode.Create(
            cmd.Code, cmd.Name, cmd.Category,
            cmd.SlaMinutes, cmd.RequiresApproval, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.ReasonCode;
    }
}
