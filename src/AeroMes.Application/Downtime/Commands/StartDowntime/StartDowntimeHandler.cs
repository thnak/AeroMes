using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Downtime.Commands.StartDowntime;

public class StartDowntimeHandler(
    IMachineRepository machineRepo,
    IDowntimeLogRepository downtimeRepo,
    IUnitOfWork uow)
    : ICommandHandler<StartDowntimeCommand, long>
{
    public async Task<long> HandleAsync(StartDowntimeCommand cmd, CancellationToken ct)
    {
        if (!await machineRepo.ExistsAsync(cmd.MachineCode, ct))
            throw new EntityNotFoundException("Machine", cmd.MachineCode);

        var log = DowntimeLog.Create(
            cmd.MachineCode, cmd.ReasonCode, cmd.ReasonName,
            cmd.StartTime, cmd.OperatorId, cmd.Notes);

        await downtimeRepo.AddAsync(log, ct);
        await uow.SaveChangesAsync(ct);

        return log.DowntimeLogID;
    }
}
