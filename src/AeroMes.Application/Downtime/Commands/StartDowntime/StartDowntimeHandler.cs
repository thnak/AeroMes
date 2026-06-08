using AeroMes.Application.Interfaces;
using AeroMes.Domain.Equipment;
using AeroMes.Domain.Equipment.Repositories;
using AeroMes.Domain.Exceptions;
using MediatR;

namespace AeroMes.Application.Downtime.Commands.StartDowntime;

public class StartDowntimeHandler(
    IWorkCenterRepository workCenterRepo,
    IDowntimeLogRepository downtimeRepo,
    IUnitOfWork uow)
    : IRequestHandler<StartDowntimeCommand, long>
{
    public async Task<long> Handle(StartDowntimeCommand cmd, CancellationToken ct)
    {
        if (!await workCenterRepo.ExistsAsync(cmd.WorkCenterId, ct))
            throw new EntityNotFoundException(nameof(WorkCenter), cmd.WorkCenterId);

        var log = DowntimeLog.Create(
            cmd.WorkCenterId, cmd.MachineCode, cmd.ReasonCode,
            cmd.ReasonName, cmd.StartTime, cmd.OperatorId);

        await downtimeRepo.AddAsync(log, ct);
        await uow.SaveChangesAsync(ct);

        return log.DowntimeLogID;
    }
}
